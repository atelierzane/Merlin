using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MerlinBackOffice.Helpers;

using MerlinBackOffice.Windows;
using MerlinBackOffice.Windows.ReportsWindows;
using MerlinBackOffice.Windows.TradeHoldWindows;

namespace MerlinBackOffice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        ApplicationHelper applicationHelper = new ApplicationHelper();
        public MainMenu()
        {
            InitializeComponent();
        }

        private void OnBtnInventory_Click(object sender, RoutedEventArgs e)
        {
            InventoryMainWindow inventoryMainWindow = new InventoryMainWindow();
            inventoryMainWindow.ShowDialog();
        }

        private void OnBtnConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationWindow configurationWindow = new ConfigurationWindow();
            configurationWindow.ShowDialog();
        }

        private void OnBtnHumanResources_Click(object sender, RoutedEventArgs e)
        {
            HumanResourcesWindow humanResourcesMainWindow = new HumanResourcesWindow();
            humanResourcesMainWindow.ShowDialog();
        }

        private void OnBtnAdministratorOptions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string merlinAdministratorPath = applicationHelper.GetMerlinAdministratorPath();
                Process.Start(merlinAdministratorPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching Merlin Administrator: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnBtnReports_Click(object sender, RoutedEventArgs e)
        {
            TransactionHistoryWindow transactionHistoryWindow = new TransactionHistoryWindow();
            transactionHistoryWindow.ShowDialog();
        }

        private void OnBtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnBtnTradeHold_Click(object sender, RoutedEventArgs e)
        {
            TradeHoldWindow tradeHoldWindow = new TradeHoldWindow();
            tradeHoldWindow.ShowDialog();
        }
    }
}