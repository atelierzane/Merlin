using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Media.Animation;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Windows.DialogWindows;
using MerlinPointOfSale.Style.Class;
using MerlinPointOfSale.Controls;

namespace MerlinPointOfSale.Windows.DialogWindows;

public partial class AddCustomerWindow : Window
{
    private InputHelper inputHelper;
    private VisualEffectsHelper visualEffectsHelper;
    private DatabaseHelper databaseHelper = new DatabaseHelper();

    public AddCustomerWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;

    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize VisualEffectsHelper
        visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
        inputHelper = new InputHelper(this, visualEffectsHelper);
        // Apply the Acrylic Blur Effect
        var blurEffect = new WindowBlurEffect(this) { BlurOpacity = 0.85 };

        // Trigger the border glow effect on window load
        visualEffectsHelper.AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));

        var titleBar = this.FindName("windowTitleBar") as DialogWindowTitleBar;
        if (titleBar != null)
        {
            titleBar.Title = "Add Customer";
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

    // Save customer to the database
    private void OnSaveCustomer_Click(object sender, RoutedEventArgs e)
    {
        // Validate the input fields
        if (string.IsNullOrEmpty(txtFirstName.Text) || string.IsNullOrEmpty(txtLastName.Text) || string.IsNullOrEmpty(txtPhoneNumber.Text) || string.IsNullOrEmpty(txtEmail.Text))
        {
            MessageBox.Show("Please fill in all required fields.");
            return;
        }

        // Generate a unique CustomerID (you can use a GUID or your own logic)
        string newCustomerID = GenerateUniqueCustomerID();

        // Create a new customer object
        Customer newCustomer = new Customer
        {
            CustomerID = newCustomerID,
            CustomerFirstName = txtFirstName.Text.Trim(),
            CustomerLastName = txtLastName.Text.Trim(),
            CustomerPhoneNumber = txtPhoneNumber.Text.Trim(),
            CustomerEmail = txtEmail.Text.Trim(),
            CustomerStreetAddress = txtAddress.Text.Trim(),
            CustomerCity = txtCity.Text.Trim(),
            CustomerState = txtState.Text.Trim(),
            CustomerZIP = txtZIP.Text.Trim(),
            CustomerLoyalty = false,  // Default values for loyalty program
            CustomerLoyaltyPaid = false,
            CustomerPoints = 0,
            CustomerStoreCredit = 0.0m,
            CustomerLoyaltyExpiration = DateTime.Now.AddYears(1)  // Default expiration date
        };

        // Add the customer to the database
        AddCustomerToDatabase(newCustomer);

        MessageBox.Show("Customer successfully added!");

        // Close the window after saving
        this.DialogResult = true;
        this.Close();
    }

    // Cancel and close the window
    private void OnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }

    // Method to generate a unique CustomerID
    private string GenerateUniqueCustomerID()
    {
        // Example: Generate a GUID as the CustomerID
        return Guid.NewGuid().ToString().Substring(0, 8).ToUpper(); // Take the first 8 characters of a GUID (customize as needed)
    }

    // Method to add a customer to the database
    private void AddCustomerToDatabase(Customer customer)
    {
        string sql = @"INSERT INTO Customers (CustomerID, CustomerFirstName, CustomerLastName, CustomerPhoneNumber, CustomerEmail, 
                                                   CustomerStreetAddress, CustomerCity, CustomerState, CustomerZIP, 
                                                   CustomerLoyalty, CustomerLoyaltyPaid, CustomerPoints, CustomerStoreCredit, 
                                                   CustomerLoyaltyExpiration)
                           VALUES (@CustomerID, @FirstName, @LastName, @Phone, @Email, @Address, @City, @State, @ZIP, @Loyalty, @LoyaltyPaid, 
                                   @Points, @StoreCredit, @LoyaltyExpiration)";

        using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                cmd.Parameters.AddWithValue("@FirstName", customer.CustomerFirstName);
                cmd.Parameters.AddWithValue("@LastName", customer.CustomerLastName);
                cmd.Parameters.AddWithValue("@Phone", customer.CustomerPhoneNumber);
                cmd.Parameters.AddWithValue("@Email", customer.CustomerEmail);
                cmd.Parameters.AddWithValue("@Address", customer.CustomerStreetAddress);
                cmd.Parameters.AddWithValue("@City", customer.CustomerCity);
                cmd.Parameters.AddWithValue("@State", customer.CustomerState);
                cmd.Parameters.AddWithValue("@ZIP", customer.CustomerZIP);
                cmd.Parameters.AddWithValue("@Loyalty", customer.CustomerLoyalty);
                cmd.Parameters.AddWithValue("@LoyaltyPaid", customer.CustomerLoyaltyPaid);
                cmd.Parameters.AddWithValue("@Points", customer.CustomerPoints);
                cmd.Parameters.AddWithValue("@StoreCredit", customer.CustomerStoreCredit);
                cmd.Parameters.AddWithValue("@LoyaltyExpiration", customer.CustomerLoyaltyExpiration);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
