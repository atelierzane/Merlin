using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Controls;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class AddAppointmentWindow : Window
    {
        private ApplicationHelper applicationHelper;
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;

        public AddAppointmentWindow(DateTime selectedDate)
        {
            InitializeComponent();
            LoadEmployees();
            LoadServices();
            LoadAddOns();
            LoadFees();
            LoadTimeSlots();

            // Set the selected date in the DatePicker
            AppointmentDatePicker.SelectedDate = selectedDate;
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize VisualEffectsHelper
            visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
            inputHelper = new InputHelper(this, visualEffectsHelper);
            applicationHelper = new ApplicationHelper();

            // Apply the Acrylic Blur Effect
            var blurEffect = new WindowBlurEffect(this) { BlurOpacity = 0.85 };

            // Trigger the border glow effect on window load
            visualEffectsHelper.AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));

            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top - 15,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.55),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };


            var titleBar = this.FindName("windowTitleBar") as WindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Updated Title - Merlin ROS";
            }

            // Optional: Add a slight delay before starting content animations
            var contentGridAnimation = FindResource("DialogWindowAnimation_NoFlash") as Storyboard;
            if (contentGridAnimation != null)
            {
                contentGridAnimation.Begin(this);
            }
            this.BeginAnimation(Window.TopProperty, topAnimation);
        }

        private void LoadTimeSlots()
        {
            var timeSlots = new List<string>();
            DateTime startTime = DateTime.Today;
            DateTime endTime = startTime.AddDays(1); // End at midnight of the next day

            while (startTime < endTime)
            {
                timeSlots.Add(startTime.ToString("hh:mm tt")); // Format as "12:00 AM"
                startTime = startTime.AddMinutes(10); // Increment by 10 minutes
            }

            AppointmentTimeComboBox.ItemsSource = timeSlots;
            AppointmentTimeComboBox.SelectedIndex = 0; // Default to the first time slot
        }

        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT EmployeeID, EmployeeFirstName, EmployeeLastName FROM Employees";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var employees = new List<Employee>();
                            while (reader.Read())
                            {
                                employees.Add(new Employee
                                {
                                    EmployeeID = reader["EmployeeID"].ToString(),
                                    FirstName = reader["EmployeeFirstName"].ToString(),
                                    LastName = reader["EmployeeLastName"].ToString()
                                });
                            }

                            EmployeeComboBox.ItemsSource = employees;
                            EmployeeComboBox.DisplayMemberPath = "FirstName"; // Show first name only
                            EmployeeComboBox.SelectedValuePath = "EmployeeID"; // Value to bind
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ServiceID, ServiceName FROM Services";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var services = new List<Service>();
                            while (reader.Read())
                            {
                                services.Add(new Service
                                {
                                    ServiceID = reader["ServiceID"].ToString(),
                                    ServiceName = reader["ServiceName"].ToString()
                                });
                            }

                            ServicesListBox.ItemsSource = services; // Update the ListBox name
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading services: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAddOns()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ServicePlusID, ServicePlusName FROM ServiceAddOns";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var addOns = new List<ServiceAddOn>();
                            while (reader.Read())
                            {
                                addOns.Add(new ServiceAddOn
                                {
                                    ServicePlusID = reader["ServicePlusID"].ToString(),
                                    ServicePlusName = reader["ServicePlusName"].ToString()
                                });
                            }

                            AddOnsListBox.ItemsSource = addOns; // Update the ListBox name
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading add-ons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFees()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ServiceFeeID, ServiceFeeName FROM ServiceFees";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var fees = new List<ServiceFee>();
                            while (reader.Read())
                            {
                                fees.Add(new ServiceFee
                                {
                                    ServiceFeeID = reader["ServiceFeeID"].ToString(),
                                    ServiceFeeName = reader["ServiceFeeName"].ToString()
                                });
                            }

                            FeesListBox.ItemsSource = fees; // Update the ListBox name
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading fees: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Collect input data
                string firstName = CustomerFirstNameTextBox.Text.Trim();
                string lastName = CustomerLastNameTextBox.Text.Trim();
                string phoneNumber = CustomerPhoneNumberTextBox.Text.Trim();
                string email = CustomerEmailTextBox.Text.Trim();
                DateTime appointmentDate = AppointmentDatePicker.SelectedDate ?? DateTime.MinValue;
                string appointmentTimeString = AppointmentTimeComboBox.SelectedItem?.ToString();
                string employeeID = EmployeeComboBox.SelectedValue?.ToString();
                string selectedServices = string.Join(", ", ServicesListBox.SelectedItems.Cast<Service>().Select(s => s.ServiceName));
                string selectedAddOns = string.Join(", ", AddOnsListBox.SelectedItems.Cast<ServiceAddOn>().Select(a => a.ServicePlusName));
                string selectedFees = string.Join(", ", FeesListBox.SelectedItems.Cast<ServiceFee>().Select(f => f.ServiceFeeName));

                // Validate inputs
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(employeeID) ||
                    string.IsNullOrWhiteSpace(appointmentTimeString))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Parse appointment time
                if (!DateTime.TryParseExact(appointmentTimeString, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out DateTime appointmentDateTime))
                {
                    MessageBox.Show("Invalid time format. Please select a valid time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TimeSpan appointmentTime = appointmentDateTime.TimeOfDay;

                // Generate AppointmentID
                string appointmentID = $"APT{new Random().Next(100000, 999999)}";

                // Insert the appointment into the database
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                INSERT INTO Appointments 
                (AppointmentID, AppointmentCustomerFirstName, AppointmentCustomerLastName, AppointmentCustomerPhoneNumber, AppointmentCustomerEmail,
                 AppointmentDate, AppointmentTime, AppointmentEmployeeID, AppointmentServices, AppointmentServiceAddOns, AppointmentServiceFees) 
                VALUES 
                (@AppointmentID, @FirstName, @LastName, @PhoneNumber, @Email, @Date, @Time, @EmployeeID, @Services, @AddOns, @Fees)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Date", appointmentDate);
                        cmd.Parameters.AddWithValue("@Time", appointmentTime); // Use TimeSpan
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        cmd.Parameters.AddWithValue("@Services", selectedServices);
                        cmd.Parameters.AddWithValue("@AddOns", selectedAddOns);
                        cmd.Parameters.AddWithValue("@Fees", selectedFees);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Appointment saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving appointment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
