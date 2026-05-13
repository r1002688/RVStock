using RVStock.Services;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;

namespace RVStock
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _api = new();

        public MainWindow()
        {
            InitializeComponent();
            BarcodeTextBox.Focus();
            _ = LaadOnderdelenAsync();
        }

        private async void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string barcode = BarcodeTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(barcode))
                {
                    try
                    {
                        var response = await _api.ScanAsync(barcode);
                        if (response.IsSuccessStatusCode)
                        {
                            ScanStatusText.Foreground = System.Windows.Media.Brushes.Green;
                            ScanStatusText.Text = $"✔  '{barcode}' uitgescand";
                            await LaadOnderdelenAsync();
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            ScanStatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                            ScanStatusText.Text = $"⚠  Barcode '{barcode}' niet gevonden";
                        }
                        else
                        {
                            ScanStatusText.Foreground = System.Windows.Media.Brushes.Red;
                            ScanStatusText.Text = "✖  Fout bij scannen";
                        }
                    }
                    catch
                    {
                        ScanStatusText.Foreground = System.Windows.Media.Brushes.Red;
                        ScanStatusText.Text = "✖  Geen verbinding met API";
                    }

                    BarcodeTextBox.Clear();
                    BarcodeTextBox.Focus();
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LaadOnderdelenAsync();

        private void BeheerButton_Click(object sender, RoutedEventArgs e)
        {
            new BeheerWindow().ShowDialog();
            _ = LaadOnderdelenAsync();
        }

        private async Task LaadOnderdelenAsync()
        {
            try
            {
                var onderdelen = await _api.GetOnderdelenAsync();
                ProductsGrid.ItemsSource = onderdelen;
                AantalItemsText.Text = $"{onderdelen?.Count ?? 0} onderdelen";
            }
            catch
            {
                // API niet bereikbaar
            }
        }
    }
}
