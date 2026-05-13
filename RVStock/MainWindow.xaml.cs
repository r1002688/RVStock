using RVStock.Services;
using System.Windows;
using System.Windows.Input;

namespace RVStock
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Toon versienummer in de topbalk
            var versie = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            VersieText.Text = $"v{versie?.Major}.{versie?.Minor}.{versie?.Build}";

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
                        await StockService.ScanAsync(barcode);
                        ScanStatusText.Foreground = System.Windows.Media.Brushes.Green;
                        ScanStatusText.Text = $"✔  '{barcode}' uitgescand";
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
            var onderdelen = await StockService.GetOnderdelenAsync();
            ProductsGrid.ItemsSource = onderdelen;
            AantalItemsText.Text = $"{onderdelen.Count} onderdelen";
        }
    }
}
