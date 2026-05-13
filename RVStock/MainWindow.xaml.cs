using RVStock.Services;
using RVStockSHARED.Models;
using System.Windows;
using System.Windows.Input;

namespace RVStock
{
    public partial class MainWindow : Window
    {
        private int? _geselecteerdeCategorieId;

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
            // Voeg "Alle" optie toe bovenaan
            var alles = new List<object> { new { Id = (int?)null, Naam = "— Alle categorieën —" } };
            alles.AddRange(categorieën.Cast<object>());
            CategorieFilterBox.ItemsSource = alles;
            CategorieFilterBox.SelectedIndex = 0;
        }

        private async void CategorieFilterBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CategorieFilterBox.SelectedItem is Categorie c)
                _geselecteerdeCategorieId = c.Id;
            else
                _geselecteerdeCategorieId = null;

            await LaadOnderdelenAsync();
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
            string barcode = BarcodeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            if (!int.TryParse(AantalScanBox.Text.Trim(), out int aantal) || aantal < 1)
            {
                ScanStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                ScanStatusText.Text = "⚠  Ongeldig aantal";
                return;
            }

            try
            {
                // Altijd scannen over alle categorieën, ongeacht de actieve filter
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
            var onderdelen = await StockService.GetOnderdelenAsync(_geselecteerdeCategorieId);
            ProductsGrid.ItemsSource = onderdelen;
            AantalItemsText.Text = $"{onderdelen.Count} onderdelen";
        }
    }
}
