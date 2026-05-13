using Microsoft.EntityFrameworkCore;
using RVStockAPI.Data;
using RVStockAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<StockContext>(options =>
    options.UseSqlite("Data Source=stock.db"));

// Achtergrondservice voor automatische e-mails
builder.Services.AddHostedService<BestellijstEmailService>();

var app = builder.Build();

// Maak de database aan bij opstart als deze nog niet bestaat
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StockContext>();

    // Maak DB aan als die nog niet bestaat
    db.Database.EnsureCreated();

    // Voeg Bestelnummer kolom toe als die nog ontbreekt (schema update)
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Onderdelen ADD COLUMN Bestelnummer TEXT NOT NULL DEFAULT ''");
    }
    catch
    {
        // Kolom bestaat al, geen probleem
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
