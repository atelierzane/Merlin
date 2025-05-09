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

using MerlinAdministrator.Pages;
using MerlinAdministrator.Pages.LandingPages;
using MerlinAdministrator.Pages.CatalogManagerPages;
using MerlinAdministrator.Pages.DepartmentManagerPages;
using MerlinAdministrator.Pages.ServicesManagerPages;
using MerlinAdministrator.Pages.VendorManagerPages;
using MerlinAdministrator.Pages.InventoryManagerPages;
using MerlinAdministrator.Pages.PromotionManagerPages;
using MerlinAdministrator.Pages.LoyaltyManagerPages;
using MerlinAdministrator.Pages.LocationManagerPages;
using MerlinAdministrator.Pages.OrganizationManagerPages;
using MerlinAdministrator.Pages.EmployeeManagerPages;
using MerlinAdministrator.Pages.PayrollPages;
using MerlinAdministrator.Pages.KPIManager;

using System.Windows.Media.Animation;

namespace MerlinAdministrator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Frame _activeFrame;
        public MainWindow()
        {
            InitializeComponent();
            _activeFrame = mainFrame; // default
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Fade in the window
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(Window.OpacityProperty, fade);

            // Animate the window rising from slightly below
            var floatUp = new DoubleAnimation
            {
                From = this.Top + 30,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(Window.TopProperty, floatUp);
        }

        private void AnimateGridColumnWidth(ColumnDefinition column, double to, double durationSeconds = 0.35)
        {
            var from = new GridLength(column.ActualWidth, GridUnitType.Pixel);
            var toGridLength = new GridLength(to, GridUnitType.Pixel);

            var animation = new GridLengthAnimation
            {
                From = from,
                To = toGridLength,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var clock = animation.CreateClock();
            clock.CurrentTimeInvalidated += (s, e) =>
            {
                // Force layout updates during the animation
                MainGrid.UpdateLayout();
            };

            column.ApplyAnimationClock(ColumnDefinition.WidthProperty, clock);
        }




        private void CatalogSearch_Checked(object sender, RoutedEventArgs e)
        {
            double totalWidth = MainGrid.ActualWidth;

            AnimateGridColumnWidth(MainFrameColumn, totalWidth * 0.5);
            AnimateGridColumnWidth(SideFrameColumn, totalWidth * 0.5);

            sideFrame.Content = new CatalogSearchPage();
        }

        private void EmployeeSearch_Checked(object sender, RoutedEventArgs e)
        {
            double totalWidth = MainGrid.ActualWidth;

            AnimateGridColumnWidth(MainFrameColumn, totalWidth * 0.5);
            AnimateGridColumnWidth(SideFrameColumn, totalWidth * 0.5);

            sideFrame.Content = new EmployeeSearchPage();
        }

        private void ToggleDualPaneButton_Unchecked(object sender, RoutedEventArgs e)
        {
            double totalWidth = MainGrid.ActualWidth;

            AnimateGridColumnWidth(MainFrameColumn, totalWidth);  // go back to 100%
            AnimateGridColumnWidth(SideFrameColumn, 0);           // collapse right pane

            sideFrame.Content = null;
        }

        private void CatalogSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            double totalWidth = MainGrid.ActualWidth;

            AnimateGridColumnWidth(MainFrameColumn, totalWidth);  // go back to 100%
            AnimateGridColumnWidth(SideFrameColumn, 0);           // collapse right pane

            sideFrame.Content = null;
        }

        private void EmployeeSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            double totalWidth = MainGrid.ActualWidth;

            AnimateGridColumnWidth(MainFrameColumn, totalWidth);  // go back to 100%
            AnimateGridColumnWidth(SideFrameColumn, 0);           // collapse right pane

            sideFrame.Content = null;
        }

        private void OnMainFrameSelected(object sender, RoutedEventArgs e)
        {
            SetActiveFrame(mainFrame);
        }

        private void OnSideFrameSelected(object sender, RoutedEventArgs e)
        {
            SetActiveFrame(sideFrame);
        }


        private void SetActiveFrame(Frame target)
        {
            _activeFrame = target;
        }

        private void OnBtnCatalogManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new CatalogManagerLandingPage();
        }

        private void OnBtnDepartmentManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new DepartmentManagerLandingPage();
        }

        private void OnBtnLocationManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new LocationManagerLandingPage();
        }

        private void OnBtnEmployeeManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EmployeeManagerLandingPage();
        }

        private void OnBtnOrganizationManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new OrganizationManagerLandingPage();
        }

        private void OnBtnVendorManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content= new VendorManagerLandingPage();
        }

        private void OnBtnInventoryManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new InventoryManagerLandingPage();
        }

        private void OnBtnPromotionManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new PromotionManagerLandingPage();
        }

        private void OnBtnLoyaltyManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new LoyaltyManagerLandingPage();
        }

        private void OnBtnKPIManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new KPIManagerLandingPage();
        }

        private void OnBtnPayroll_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new PayrollLandingPage();
        }

        private void OnBtnServicesManager_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new ServicesManagerLandingPage();
        }



        private void Menu_Catalog_Products_AddProduct_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddProductPage();
        }

        private void Menu_Catalog_Products_EditProduct_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditProductPage();
        }

        private void Menu_Catalog_Products_RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveProductPage();
        }

        private void Menu_Catalog_Products_BulkEdit_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditProductBulkPage();
        }

        private void Menu_Catalog_Products_BulkRemove_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveProductBulkPage();
        }


        private void Menu_Catalog_CatalogSearch_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new CatalogSearchPage();
        }

        private void Menu_Catalog_Departments_AddDepartment_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddDepartmentPage();
        }

        private void Menu_Catalog_Departments_EditDepartment_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditDepartmentPage();
        }

        private void Menu_Catalog_Departments_DepartmentMap_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new ViewDepartmentsPage();
        }

        private void Menu_Catalog_ServicesAndFees_AddService_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddServicePage();
        }

        private void Menu_Catalog_ServicesAndFees_RemoveService_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveServiceBulkPage();
        }

        private void Menu_Catalog_ServicesAndFees_AddServiceAddOn_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddServiceAddOnsPage();
        }

        private void Menu_Catalog_ServicesAndFees_AddServiceFee_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddServiceFeePage();
        }

        private void Menu_Catalog_ServicesAndFees_ServiceSearch_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new ServiceSearchPage();
        }

        private void Menu_Catalog_Vendors_AddVendor_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddVendorPage();
        }

        private void Menu_Catalog_Vendors_EditVendor_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditVendorPage();
        }
        private void Menu_Catalog_Vendors_RemoveVendor_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveVendorPage();
        }

        private void Menu_Catalog_Vendors_VendorSearch_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new VendorSearchPage();
        }

        private void Menu_Catalog_ImportExport_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new DataMigrationPage();
        }

        private void Menu_Catalog_Export_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new CatalogExportPage();
        }
        private void Menu_Inventory_ShippingReceiving_CreateCarton_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new CartonCreationPage();
        }

        private void Menu_Inventory_ShippingReceiving_EditCarton_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditCartonPage();
        }

        private void Menu_Inventory_ShippingReceiving_DeleteCarton_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveCartonPage();
        }

        private void Menu_Inventory_ShippingReceiving_SearchCartons_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new CartonSearchPage();
        }

        private void Menu_Inventory_InventorySearch_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new InventorySearchPage();
        }

        private void Menu_Promotions_ProductBundles_AddProductBundle_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddComboPage();
        }

        private void Menu_Promotions_ProductBundles_EditProductBundle_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditComboPage();
        }

        private void Menu_Promotions_ProductBundles_RemoveProductBundle_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveComboPage();
        }

        private void Menu_Promotions_ProductBundles_BulkRemove_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveComboBulkPage();
        }

        private void Menu_Promotions_ProductBundles_SearchProductBunles_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new ComboSearchPage();
        }

        private void Menu_Promotions_Promotions_AddPromotion_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddPromotionPage();
        }

        private void Menu_Promotions_Promotions_EditPromotion_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditPromotionPage();
        }

        private void Menu_Promotions_Promotions_RemovePromotion_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemovePromotionPage();
        }

        private void Menu_Promotions_Promotions_BulkRemove_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemovePromotionBulkPage();
        }

        private void Menu_Promotions_Promotions_SearchPromotions_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new PromotionSearchPage();
        }

        private void Menu_Promotions_Loyalty_EditLoyaltyProgram_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddLoyaltyPage();
        }

        private void Menu_Promotions_Loyalty_LoyaltyProgramSearch_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new LoyaltySearchPage();
        }

        private void Menu_Organization_Add_Location_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddLocationPage();
        }

        private void Menu_Organization_Add_District_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddDistrictPage();
        }

        private void Menu_Organization_Add_Region_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddRegionPage();
        }

        private void Menu_Organization_Add_Market_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddMarketPage();
        }

        private void Menu_Organization_Add_Division_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddDivisionPage();
        }

        private void Menu_Organization_Edit_Location_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditLocationPage();
        }

        private void Menu_Organization_Edit_District_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditDistrictPage();
        }

        private void Menu_Organization_Edit_Region_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditRegionPage();
        }

        private void Menu_Organization_Edit_Market_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditMarketPage();
        }

        private void Menu_Organization_Edit_Division_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditDivisionPage();
        }

        private void Menu_Organization_RemoveLocation_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveLocationPage();
        }

        private void Menu_Organization_OrganizationChart_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new OrganizationChart();
        }

        private void Menu_Organization_LocationSearch_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new LocationSearchPage();
        }

        private void Menu_HumanResources_Employees_AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddEmployeePage();
        }

        private void Menu_HumanResources_Employees_EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EditEmployeePage();
        }

        private void Menu_HumanResources_Employees_RemoveEmployee_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new RemoveEmployeePage();
        }

        private void Menu_HumanResources_Employees_EmployeeSearch_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new EmployeeSearchPage();
        }

        private void Menu_HumanResources_Payroll_AllocateHours_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AllocateHoursPage();
        }

        private void Menu_Performance_KPIs_AddCustomKPI_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new AddKPIPage();
        }

        private void Menu_Performance_KPIs_ViewAll_Click(object sender, RoutedEventArgs e)
        {
            _activeFrame.Content = new KPISearchPage();
        }


        private void Menu_File_Configuration_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationWindow configurationWindow = new ConfigurationWindow();
            configurationWindow.ShowDialog();
        }

    }
}