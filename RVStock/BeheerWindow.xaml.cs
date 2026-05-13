using RVStock.Services;
using RVStockSHARED.Models;
using System.Windows;
using System.Windows.Controls;

namespace RVStock
{
    public partial class BeheerWindow : Window
    {
        private int? _geselecteerdeLeverancierId;
        private int? _geselecteerdOnderdeelId;
        private int? _geselecteerdeCategorieId;

        public BeheerWindow()
        {
            InitializeComponent();
            _ = LaadAllesAsync();
        }

        private async Task LaadAllesAsync()
        {
            await LaadLeveranciersAsync();
            await LaadCategorieënAsync();
            await LaadOnderdelenAsync();
            await LaadBestelijnenAsync();
        }

        // ===================== LEVERANCIERS =====================

        private async Task LaadLeveranciersAsync()
        {
            var lijst = await StockService.GetLeveranciersAsync();
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
                foreach (ComboBoxItem item in LevBesteldagBox.Items)
                    if (int.Parse(item.Tag.ToString()!) == (int)l.BestelDag)
                    { LevBesteldagBox.SelectedItem = item; break; }
            }
        }

        private async void SaveLeverancier_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LevNaamBox.Text) || LevBesteldagBox.SelectedItem == null) return;
            var tagWaarde = int.Parse(((ComboBoxItem)LevBesteldagBox.SelectedItem).Tag.ToString()!);
            var dag = (DayOfWeek)tagWaarde;
            var leverancier = new Leverancier
            {
                Id = _geselecteerdeLeverancierId ?? 0,
                Naam = LevNaamBox.Text.Trim(),
                Email = LevEmailBox.Text.Trim(),
                BestelDag = dag
            };
            await StockService.SaveLeverancierAsync(leverancier);
            await LaadLeveranciersAsync();
            ClearLeverancierForm();
        }

        private void NieuweLeverancier_Click(object sender, RoutedEventArgs e) => ClearLeverancierForm();

        private async void DeleteLeverancier_Click(object sender, RoutedEventArgs e)
        {
            if (!_geselecteerdeLeverancierId.HasValue) return;
            if (MessageBox.Show("Leverancier verwijderen?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            await StockService.DeleteLeverancierAsync(_geselecteerdeLeverancierId.Value);
            await LaadLeveranciersAsync();
            ClearLeverancierForm();
        }

        private async void RefreshLeveranciers_Click(object sender, RoutedEventArgs e) => await LaadLeveranciersAsync();

        private void ClearLeverancierForm()
        {
            _geselecteerdeLeverancierId = null;
            LevNaamBox.Clear(); LevEmailBox.Clear();
            LevBesteldagBox.SelectedIndex = 0;
            LeveranciersGrid.SelectedItem = null;
        }

        // ===================== ONDERDELEN =====================

        private async Task LaadOnderdelenAsync()
        {
            var lijst = await StockService.GetOnderdelenAsync();
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
                OndCategorieBox.SelectedValue = o.CategorieId;
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
                LeverancierId = (int)(OndLeverancierBox.SelectedValue ?? 0),
                CategorieId = OndCategorieBox.SelectedValue as int?
            };
            await StockService.SaveOnderdeelAsync(onderdeel);
            await LaadOnderdelenAsync();
            ClearOnderdeelForm();
        }

        private void NieuwOnderdeel_Click(object sender, RoutedEventArgs e) => ClearOnderdeelForm();

        private async void DeleteOnderdeel_Click(object sender, RoutedEventArgs e)
        {
            if (!_geselecteerdOnderdeelId.HasValue) return;
            if (MessageBox.Show("Onderdeel verwijderen?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            await StockService.DeleteOnderdeelAsync(_geselecteerdOnderdeelId.Value);
            await LaadOnderdelenAsync();
            ClearOnderdeelForm();
        }

        private async void RefreshOnderdelen_Click(object sender, RoutedEventArgs e) => await LaadOnderdelenAsync();

        private void ClearOnderdeelForm()
        {
            _geselecteerdOnderdeelId = null;
            OndBestelnummerBox.Clear(); OndNaamBox.Clear();
            OndBarcodeBox.Clear(); OndVoorraadBox.Clear();
            OndLeverancierBox.SelectedIndex = -1;
            OndCategorieBox.SelectedIndex = -1;
            OnderdelenGrid.SelectedItem = null;
        }

        // ===================== CATEGORIEËN =====================

        private async Task LaadCategorieënAsync()
        {
            var lijst = await StockService.GetCategorieënAsync();
            CategorieënGrid.ItemsSource = lijst;
            OndCategorieBox.ItemsSource = lijst;
        }

        private void CategorieënGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorieënGrid.SelectedItem is Categorie c)
            {
                _geselecteerdeCategorieId = c.Id;
                CatNaamBox.Text = c.Naam;
            }
        }

        private async void SaveCategorie_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CatNaamBox.Text)) return;
            var categorie = new Categorie
            {
                Id = _geselecteerdeCategorieId ?? 0,
                Naam = CatNaamBox.Text.Trim()
            };
            await StockService.SaveCategorieAsync(categorie);
            await LaadCategorieënAsync();
            ClearCategorieForm();
        }

        private void NieuweCategorie_Click(object sender, RoutedEventArgs e) => ClearCategorieForm();

        private async void DeleteCategorie_Click(object sender, RoutedEventArgs e)
        {
            if (!_geselecteerdeCategorieId.HasValue) return;
            if (MessageBox.Show("Categorie verwijderen?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            await StockService.DeleteCategorieAsync(_geselecteerdeCategorieId.Value);
            await LaadCategorieënAsync();
            ClearCategorieForm();
        }

        private async void RefreshCategorieën_Click(object sender, RoutedEventArgs e) => await LaadCategorieënAsync();

        private void ClearCategorieForm()
        {
            _geselecteerdeCategorieId = null;
            CatNaamBox.Clear();
            CategorieënGrid.SelectedItem = null;
        }

        // ===================== BESTELIJNEN =====================

        private async Task LaadBestelijnenAsync()
        {
            var lijst = await StockService.GetOpenBestelijnenAsync();
            BestelijnenGrid.ItemsSource = lijst;
        }

        private async void RefreshBestelijnen_Click(object sender, RoutedEventArgs e) => await LaadBestelijnenAsync();
    }
}
