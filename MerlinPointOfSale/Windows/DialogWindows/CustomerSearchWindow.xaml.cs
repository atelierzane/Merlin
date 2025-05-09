using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Windows.Media.Animation;

using MerlinPointOfSale.Controls;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using MerlinPointOfSale.Helpers;

using MerlinPointOfSale.Models;
using System.Windows.Media;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class CustomerSearchWindow : Window
    {
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;

        private DatabaseHelper databaseHelper = new DatabaseHelper();
        public Customer SelectedCustomer { get; private set; }

        public CustomerSearchWindow(string searchTerm)
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            // Prepopulate the search field
            txtSearchCustomer.Text = searchTerm;

            // Trigger the search automatically if searchTerm is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                PerformCustomerSearch(searchTerm);
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply the blur effect
            var blurEffect = new WindowBlurEffect(this);
            blurEffect.EnableBlur();


            // Initialize VisualEffectsHelper
            visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
            inputHelper = new InputHelper(this, visualEffectsHelper);

            // Trigger the border glow effect on window load
            visualEffectsHelper.AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));

            var titleBar = this.FindName("windowTitleBar") as DialogWindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Search Customers";
            }


            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top - 15,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.55),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            // Optional: Add a slight delay before starting content animations
            var contentGridAnimation = FindResource("DialogWindowAnimation_NoFlash") as Storyboard;
            if (contentGridAnimation != null)
            {
                contentGridAnimation.Begin(this);
            }

            this.BeginAnimation(Window.TopProperty, topAnimation);
        }
        private void OnSearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = txtSearchCustomer.Text.Trim();
            PerformCustomerSearch(searchTerm);
        }


        // Retrieve customer details from database
        private List<Customer> SearchCustomers(string searchText)
        {
            List<Customer> customers = new List<Customer>();

            string sql = @"SELECT CustomerID, CustomerFirstName, CustomerLastName, CustomerPhoneNumber, CustomerEmail 
                           FROM Customers 
                           WHERE CustomerFirstName LIKE @search OR CustomerLastName LIKE @search 
                           OR CustomerPhoneNumber LIKE @search OR CustomerEmail LIKE @search";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchText}%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customer
                            {
                                CustomerID = reader["CustomerID"].ToString(),
                                CustomerFirstName = reader["CustomerFirstName"].ToString(),
                                CustomerLastName = reader["CustomerLastName"].ToString(),
                                CustomerPhoneNumber = reader["CustomerPhoneNumber"].ToString(),
                                CustomerEmail = reader["CustomerEmail"].ToString()
                            });
                        }
                    }
                }
            }

            return customers;
        }

        private void dgCustomerResults_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            // Hit test to check if the click occurred on a DataGridRow
            var hit = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));
            if (hit != null)
            {
                // Get the row or cell under the click
                var row = VisualUpwardSearch<DataGridRow>(hit.VisualHit);

                if (row == null)
                {
                    // If not clicking on a row, clear the selection
                    dataGrid.SelectedItem = null;
                }
            }
        }

        // Utility method to find the parent of a specific type in the visual tree
        private static T VisualUpwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
            while (source != null && !(source is T))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as T;
        }


        // Confirm customer selection
        private void OnSelectCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomerResults.SelectedItem is Customer selectedCustomer)
            {
                this.SelectedCustomer = selectedCustomer;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a customer.");
            }
        }

        // Cancel the operation
        private void OnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PerformCustomerSearch(string searchTerm)
        {
            List<Customer> customers = SearchCustomers(searchTerm);
            dgCustomerResults.ItemsSource = customers;
        }
    }
}
