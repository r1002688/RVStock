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
                            await LaadOnderdelenAsync();
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            MessageBox.Show($"Geen onderdeel gevonden met barcode '{barcode}'.", "Niet gevonden", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Er is een fout opgetreden bij het scannen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Kan geen verbinding maken met de API.\n{ex.Message}", "Verbindingsfout", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
            catch
            {
                // API niet bereikbaar
            }
        }
    }
}
