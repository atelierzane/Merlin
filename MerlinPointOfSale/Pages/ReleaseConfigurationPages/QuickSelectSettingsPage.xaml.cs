using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Pages.ReleaseConfigurationPages
{
    public partial class QuickSelectSettingsPage : Page
    {
        public QuickSelectSettingsPage()
        {
            InitializeComponent();
        }

        private void OnAddIndividualItem_Click(object sender, RoutedEventArgs e)
        {
            lstIndividualItems.Items.Add($"Item {lstIndividualItems.Items.Count + 1}");
        }

        private void OnAddCombo_Click(object sender, RoutedEventArgs e)
        {
            lstCombos.Items.Add($"Combo {lstCombos.Items.Count + 1}");
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save logic for quick select settings
            MessageBox.Show($"Saved {lstIndividualItems.Items.Count} individual items and {lstCombos.Items.Count} combos!", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
