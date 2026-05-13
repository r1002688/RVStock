using RVStock.Services;
using RVStockSHARED.Models;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace RVStock
{
    public partial class BeheerWindow : Window
    {
        private readonly ApiClient _api = new();
        private int? _geselecteerdeLeverancierId;
        private int? _geselecteerdOnderdeelId;

        public BeheerWindow()
        {
            InitializeComponent();
            _ = LaadAllesAsync();
        }

        private async Task LaadAllesAsync()
        {
            await LaadLeveranciersAsync();
            await LaadOnderdelenAsync();
            await LaadBestelijnenAsync();
        }

        // ===================== LEVERANCIERS =====================

        private async Task LaadLeveranciersAsync()
        {
            var lijst = await _api.GetLeveranciersAsync();
            LeveranciersGrid.ItemsSource = lijst;
            OndLeverancierBox.ItemsSource = lijst;
        }

        private void LeveranciersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeveranciersGrid.SelectedItem is Leverancier l)
            {
                _geselecteerdeLeverancierId = l.Id;
                LevNaamBox.Text = l.Naam;
                LevEmailBox.Text = l.Email;
                // Selecteer juiste dag in ComboBox
                foreach (ComboBoxItem item in LevBesteldagBox.Items)
                    if ((int)item.Tag == (int)l.BestelDag)
                    { LevBesteldagBox.SelectedItem = item; break; }
            }
        }

        private async void SaveLeverancier_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LevNaamBox.Text) || LevBesteldagBox.SelectedItem == null) return;

            var dag = (DayOfWeek)(int)((ComboBoxItem)LevBesteldagBox.SelectedItem).Tag;
            var leverancier = new Leverancier
            {
                Id = _geselecteerdeLeverancierId ?? 0,
                Naam = LevNaamBox.Text.Trim(),
                Email = LevEmailBox.Text.Trim(),
                BestelDag = dag
            };

            HttpResponseMessage response;
            if (_geselecteerdeLeverancierId.HasValue)
                response = await _api.UpdateLeverancierAsync(leverancier);
            else
                response = await _api.CreateLeverancierAsync(leverancier);

            if (response.IsSuccessStatusCode)
            {
                await LaadLeveranciersAsync();
                ClearLeverancierForm();
            }
            else
                MessageBox.Show("Fout bij opslaan leverancier.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void NieuweLeverancier_Click(object sender, RoutedEventArgs e) => ClearLeverancierForm();

        private async void DeleteLeverancier_Click(object sender, RoutedEventArgs e)
        {
            if (!_geselecteerdeLeverancierId.HasValue) return;
            if (MessageBox.Show("Leverancier verwijderen?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            await _api.DeleteLeverancierAsync(_geselecteerdeLeverancierId.Value);
            await LaadLeveranciersAsync();
            ClearLeverancierForm();
        }

        private async void RefreshLeveranciers_Click(object sender, RoutedEventArgs e) => await LaadLeveranciersAsync();

        private void ClearLeverancierForm()
        {
            _geselecteerdeLeverancierId = null;
            LevNaamBox.Clear();
            LevEmailBox.Clear();
            LevBesteldagBox.SelectedIndex = 0;
            LeveranciersGrid.SelectedItem = null;
        }

        // ===================== ONDERDELEN =====================

        private async Task LaadOnderdelenAsync()
        {
            var lijst = await _api.GetOnderdelenAsync();
            OnderdelenGrid.ItemsSource = lijst;
        }

        private void OnderdelenGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnderdelenGrid.SelectedItem is Onderdeel o)
            {
                _geselecteerdOnderdeelId = o.Id;
                OndBestelnummerBox.Text = o.Bestelnummer;
                OndNaamBox.Text = o.Naam;
                OndBarcodeBox.Text = o.Barcode;
                OndVoorraadBox.Text = o.Voorraad.ToString();
                OndLeverancierBox.SelectedValue = o.LeverancierId;
            }
        }

        private async void SaveOnderdeel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OndNaamBox.Text)) return;
            if (!int.TryParse(OndVoorraadBox.Text, out int voorraad)) { MessageBox.Show("Ongeldige voorraad."); return; }

            var onderdeel = new Onderdeel
            {
                Id = _geselecteerdOnderdeelId ?? 0,
                Bestelnummer = OndBestelnummerBox.Text.Trim(),
                Naam = OndNaamBox.Text.Trim(),
                Barcode = OndBarcodeBox.Text.Trim(),
                Voorraad = voorraad,
                LeverancierId = (int)(OndLeverancierBox.SelectedValue ?? 0)
            };

            HttpResponseMessage response;
            if (_geselecteerdOnderdeelId.HasValue)
                response = await _api.UpdateOnderdeelAsync(onderdeel);
            else
                response = await _api.CreateOnderdeelAsync(onderdeel);

            if (response.IsSuccessStatusCode)
            {
                await LaadOnderdelenAsync();
                ClearOnderdeelForm();
            }
            else
                MessageBox.Show("Fout bij opslaan onderdeel.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void NieuwOnderdeel_Click(object sender, RoutedEventArgs e) => ClearOnderdeelForm();

        private async void DeleteOnderdeel_Click(object sender, RoutedEventArgs e)
        {
            if (!_geselecteerdOnderdeelId.HasValue) return;
            if (MessageBox.Show("Onderdeel verwijderen?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            await _api.DeleteOnderdeelAsync(_geselecteerdOnderdeelId.Value);
            await LaadOnderdelenAsync();
            ClearOnderdeelForm();
        }

        private async void RefreshOnderdelen_Click(object sender, RoutedEventArgs e) => await LaadOnderdelenAsync();

        private void ClearOnderdeelForm()
        {
            _geselecteerdOnderdeelId = null;
            OndBestelnummerBox.Clear();
            OndNaamBox.Clear();
            OndBarcodeBox.Clear();
            OndVoorraadBox.Clear();
            OndLeverancierBox.SelectedIndex = -1;
            OnderdelenGrid.SelectedItem = null;
        }

        // ===================== BESTELIJNEN =====================

        private async Task LaadBestelijnenAsync()
        {
            var lijst = await _api.GetOpenBestelijnenAsync();
            BestelijnenGrid.ItemsSource = lijst;
        }

        private async void RefreshBestelijnen_Click(object sender, RoutedEventArgs e) => await LaadBestelijnenAsync();
    }
}
