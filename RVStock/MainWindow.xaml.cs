using RVStock.Services;
using RVStockSHARED.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RVStock
{
    public partial class MainWindow : Window
    {
        private int? _geselecteerdeCategorieId;
        private List<Onderdeel> _alleOnderdelen = [];

        public MainWindow()
        {
            InitializeComponent();

            var versie = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            VersieText.Text = $"v{versie?.Major}.{versie?.Minor}.{versie?.Build}";

            BarcodeTextBox.Focus();
            _ = LaadAllesAsync();
        }

        private async Task LaadAllesAsync()
        {
            await LaadCategorieënAsync();
            await LaadOnderdelenAsync();
        }

        private async Task LaadCategorieënAsync()
        {
            var categorieën = await StockService.GetCategorieënAsync();
            var alles = new List<object> { new { Id = (int?)null, Naam = "— Alle categorieën —" } };
            alles.AddRange(categorieën.Cast<object>());
            CategorieFilterBox.ItemsSource = alles;
            CategorieFilterBox.SelectedIndex = 0;
        }

        private async void CategorieFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorieFilterBox.SelectedItem is Categorie c)
                _geselecteerdeCategorieId = c.Id;
            else
                _geselecteerdeCategorieId = null;

            await LaadOnderdelenAsync();
        }

        private void ZoekBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PasZoekfilterToe();
        }

        private void ZoekWissen_Click(object sender, RoutedEventArgs e)
        {
            ZoekBox.Clear();
            BarcodeTextBox.Focus();
        }

        private void PasZoekfilterToe()
        {
            var zoek = ZoekBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(zoek))
            {
                ProductsGrid.ItemsSource = _alleOnderdelen;
                AantalItemsText.Text = $"{_alleOnderdelen.Count} onderdelen";
            }
            else
            {
                var gefilterd = _alleOnderdelen
                    .Where(o => o.Naam.ToLower().Contains(zoek)
                             || o.Bestelnummer.ToLower().Contains(zoek)
                             || o.Barcode.ToLower().Contains(zoek))
                    .ToList();
                ProductsGrid.ItemsSource = gefilterd;
                AantalItemsText.Text = $"{gefilterd.Count} van {_alleOnderdelen.Count} onderdelen";
            }
        }

        private async void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await UitscannenAsync();
        }

        private async void UitscanButton_Click(object sender, RoutedEventArgs e)
            => await UitscannenAsync();

        private async Task UitscannenAsync()
        {
            if (!int.TryParse(AantalScanBox.Text.Trim(), out int aantal) || aantal < 1)
            {
                ScanStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                ScanStatusText.Text = "⚠  Ongeldig aantal";
                return;
            }

            // Prioriteit 1: geselecteerde rij in de lijst
            if (ProductsGrid.SelectedItem is Onderdeel geselecteerd && string.IsNullOrWhiteSpace(BarcodeTextBox.Text))
            {
                try
                {
                    await StockService.ScanByIdAsync(geselecteerd.Id, aantal);
                    ScanStatusText.Foreground = System.Windows.Media.Brushes.Green;
                    ScanStatusText.Text = $"✔  '{geselecteerd.Naam}' × {aantal} uitgescand";
                    await LaadOnderdelenAsync();
                }
                catch (Exception ex)
                {
                    ScanStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    ScanStatusText.Text = $"⚠  {ex.Message}";
                }
                return;
            }

            // Prioriteit 2: barcode of bestelnummer ingegeven
            string barcode = BarcodeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(barcode))
            {
                ScanStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                ScanStatusText.Text = "⚠  Scan een barcode of selecteer een rij";
                return;
            }

            try
            {
                await StockService.ScanAsync(barcode, aantal);
                ScanStatusText.Foreground = System.Windows.Media.Brushes.Green;
                ScanStatusText.Text = $"✔  '{barcode}' × {aantal} uitgescand";
                await LaadOnderdelenAsync();
            }
            catch (Exception ex)
            {
                ScanStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                ScanStatusText.Text = $"⚠  {ex.Message}";
            }

            BarcodeTextBox.Clear();
            BarcodeTextBox.Focus();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LaadOnderdelenAsync();

        private void BeheerButton_Click(object sender, RoutedEventArgs e)
        {
            new BeheerWindow().ShowDialog();
            _ = LaadAllesAsync();
        }

        private async Task LaadOnderdelenAsync()
        {
            _alleOnderdelen = await StockService.GetOnderdelenAsync(_geselecteerdeCategorieId);
            PasZoekfilterToe();
        }
    }
}
