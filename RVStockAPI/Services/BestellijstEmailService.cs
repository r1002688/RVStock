using Microsoft.EntityFrameworkCore;
using RVStockAPI.Data;
using RVStockSHARED.Models;
using System.Net;
using System.Net.Mail;

namespace RVStockAPI.Services
{
    public class BestellijstEmailService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<BestellijstEmailService> _logger;

        public BestellijstEmailService(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<BestellijstEmailService> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Wacht tot middernacht, daarna elke 24u controleren
                var nu = DateTime.Now;
                var volgendeCheck = nu.Date.AddDays(1); // Morgen om 00:00
                var wachttijd = volgendeCheck - nu;

                await Task.Delay(wachttijd, stoppingToken);

                await VerstuurBestellijstenAsync();
            }
        }

        private async Task VerstuurBestellijstenAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StockContext>();

            var vandaag = DateTime.Today.DayOfWeek;

            // Haal alle leveranciers op waarvan vandaag de besteldag is
            var leveranciers = await context.Leveranciers
                .Where(l => l.BestelDag == vandaag)
                .ToListAsync();

            foreach (var leverancier in leveranciers)
            {
                // Haal open bestelijnen op voor deze leverancier
                var bestelijnen = await context.Bestelijnen
                    .Where(b => b.Status == "Open" && b.Onderdeel!.LeverancierId == leverancier.Id)
                    .Include(b => b.Onderdeel)
                    .ToListAsync();

                if (!bestelijnen.Any())
                    continue; // Geen items, geen mail sturen

                try
                {
                    await StuurEmailAsync(leverancier, bestelijnen);

                    // Markeer als besteld
                    foreach (var lijn in bestelijnen)
                        lijn.Status = "Besteld";

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Bestellijst gestuurd naar {leverancier.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Fout bij versturen mail naar {leverancier.Email}");
                }
            }
        }

        private async Task StuurEmailAsync(Leverancier leverancier, List<Bestellijn> bestelijnen)
        {
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var smtpUser = _config["Email:Gebruiker"] ?? "";
            var smtpPass = _config["Email:Wachtwoord"] ?? "";
            var vanEmail = _config["Email:VanAdres"] ?? smtpUser;

            var body = $"Beste {leverancier.Naam},\n\nHieronder vindt u de bestellijst van vandaag:\n\n";
            foreach (var lijn in bestelijnen)
                body += $"- {lijn.Onderdeel!.Naam}: {lijn.Aantal} stuks\n";
            body += "\nMet vriendelijke groeten,\nRVStock";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage(vanEmail, leverancier.Email)
            {
                Subject = $"Bestellijst {DateTime.Today:dd/MM/yyyy}",
                Body = body
            };

            await client.SendMailAsync(mail);
        }
    }
}
