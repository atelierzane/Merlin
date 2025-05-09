using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Windows.DialogWindows;
using MerlinPointOfSale.Windows;
using MerlinPointOfSale.Helpers;

namespace MerlinPointOfSale.Controls
{
    public partial class CalendarControl : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<CalendarDate> Dates { get; private set; }
        private DateTime currentDate;
        private bool isNavigating; // Prevent multiple navigations
        private int pendingOffset; // Accumulated navigation offset
        private string currentMonth;
        private string currentYear;
        public DatabaseHelper databaseHelper = new DatabaseHelper();

        public string DisplayMonth => currentDate.ToString("MMMM yyyy");

        public CalendarControl()
        {
            InitializeComponent();
            Dates = new ObservableCollection<CalendarDate>();
            currentDate = DateTime.Today;
            LoadDates();
            DataContext = this;
            isNavigating = false;
            pendingOffset = 0;
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToMonth(-1, "MonthTransitionOutBackward", "MonthTransitionInBackward");
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToMonth(1, "MonthTransitionOutForward", "MonthTransitionInForward");
        }

        private void NavigateToMonth(int offset, string outAnimationKey, string inAnimationKey)
        {
            // Accumulate navigation offsets
            pendingOffset += offset;

            // If already navigating, return and let the current animation finish
            if (isNavigating) return;

            // Start processing navigation
            isNavigating = true;

            var fadeOut = (Storyboard)FindResource(outAnimationKey);
            fadeOut.Completed += (s, _) =>
            {
                // Apply the cumulative offset and reset the accumulator
                currentDate = currentDate.AddMonths(pendingOffset);
                pendingOffset = 0;

                // Reload dates and update UI
                LoadDates();
                OnPropertyChanged(nameof(DisplayMonth));

                var fadeIn = (Storyboard)FindResource(inAnimationKey);
                fadeIn.Completed += (s2, _) =>
                {
                    // Unlock navigation
                    isNavigating = false;

                    // If there are further pending offsets, trigger navigation again
                    if (pendingOffset != 0)
                    {
                        NavigateToMonth(0, outAnimationKey, inAnimationKey);
                    }
                };

                fadeIn.Begin(DayButtonsContainer);
                fadeIn.Begin(MonthNameTextBlock);
            };

            fadeOut.Begin(DayButtonsContainer);
            fadeOut.Begin(MonthNameTextBlock);
        }

        private void LoadDates()
        {
            Dates.Clear();

            DateTime firstOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

            // Determine the starting day of the week (Sunday = 0, Monday = 1, ..., Saturday = 6)
            int startDayOffset = (int)firstOfMonth.DayOfWeek;

            // Add placeholders for empty cells before the first day
            for (int i = 0; i < startDayOffset; i++)
            {
                Dates.Add(new CalendarDate { Date = DateTime.MinValue });
            }

            // Add actual days of the month
            for (int i = 1; i <= daysInMonth; i++)
            {
                Dates.Add(new CalendarDate { Date = new DateTime(currentDate.Year, currentDate.Month, i) });
            }

            // Add placeholders for empty cells after the last day
            int totalCells = 42; // 6 rows × 7 columns
            int remainingCells = totalCells - Dates.Count;
            for (int i = 0; i < remainingCells; i++)
            {
                Dates.Add(new CalendarDate { Date = DateTime.MinValue });
            }
        }

        private void DateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CalendarDate selectedDate)
            {
                ShowDayView(selectedDate);
            }
        }

        private void BackToMonthView_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = (Storyboard)FindResource("TransitionOutBackward");
            fadeOut.Completed += (s, _) =>
            {
                DayView.Visibility = Visibility.Collapsed;
                MonthView.Visibility = Visibility.Visible;

                var fadeIn = (Storyboard)FindResource("TransitionInBackward");
                fadeIn.Begin(MonthView);
            };
            fadeOut.Begin(DayView);
        }

        private void ShowDayView(CalendarDate selectedDate)
        {
            var fadeOut = (Storyboard)FindResource("TransitionOutForward");
            fadeOut.Completed += (s, _) =>
            {
                MonthView.Visibility = Visibility.Collapsed;
                DayView.Visibility = Visibility.Visible;

                // Update content for the day view
                DayViewHeader.Text = $"Appointments for {selectedDate.Date:D}";
                DayAppointmentsDataGrid.ItemsSource = LoadAppointments(selectedDate.Date);


                var fadeIn = (Storyboard)FindResource("TransitionInForward");
                fadeIn.Begin(DayView);
            };
            fadeOut.Begin(MonthView);
        }

        private ObservableCollection<Appointment> LoadAppointments(DateTime date)
        {
            var appointments = new ObservableCollection<Appointment>();

            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                                    SELECT 
                                        A.AppointmentID,
                                        A.AppointmentTime,
                                        A.AppointmentCustomerFirstName,
                                        A.AppointmentCustomerLastName,
                                        A.AppointmentCustomerPhoneNumber,
                                        A.AppointmentCustomerEmail,
                                        A.AppointmentEmployeeID,
                                        A.AppointmentServices,
                                        A.AppointmentServiceAddOns,
                                        A.AppointmentServiceFees,
                                        LTRIM(RTRIM(E.EmployeeFirstName)) + ' ' + LTRIM(RTRIM(E.EmployeeLastName)) AS EmployeeName,
                                        S.ServiceID, S.ServiceName, S.ServicePrice,
                                        SA.ServicePlusID, SA.ServicePlusName, SA.ServicePlusPrice,
                                        SF.ServiceFeeID, SF.ServiceFeeName, SF.ServiceFeePrice
                                    FROM Appointments A
                                    LEFT JOIN Employees E ON A.AppointmentEmployeeID = E.EmployeeID
                                    LEFT JOIN Services S ON A.AppointmentServices LIKE '%' + S.ServiceName + '%'
                                    LEFT JOIN ServiceAddOns SA ON A.AppointmentServiceAddOns LIKE '%' + SA.ServicePlusName + '%'
                                    LEFT JOIN ServiceFees SF ON A.AppointmentServiceFees LIKE '%' + SF.ServiceFeeName + '%'
                                    WHERE A.AppointmentDate = @Date";
;

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Date", date);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var appointment = new Appointment
                                {
                                    AppointmentID = reader["AppointmentID"].ToString(),
                                    AppointmentTime = TimeSpan.Parse(reader["AppointmentTime"].ToString()),
                                    CustomerFirstName = reader["AppointmentCustomerFirstName"].ToString().Trim(),
                                    CustomerLastName = reader["AppointmentCustomerLastName"].ToString().Trim(),
                                    CustomerPhoneNumber = reader["AppointmentCustomerPhoneNumber"].ToString(),
                                    CustomerEmail = reader["AppointmentCustomerEmail"].ToString().Trim(),
                                    EmployeeID = reader["AppointmentEmployeeID"]?.ToString(),
                                    EmployeeName = reader["EmployeeName"]?.ToString()?.Trim(),

                                    Services = reader["AppointmentServices"]?.ToString()
                                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => new Service
                                        {
                                            ServiceID = reader["ServiceID"]?.ToString(),
                                            ServiceName = s.Trim(),
                                            ServicePrice = reader["ServicePrice"] != DBNull.Value
                                                ? Convert.ToDecimal(reader["ServicePrice"])
                                                : 0
                                        })
                                        .ToList(),

                                    ServiceAddOns = reader["AppointmentServiceAddOns"]?.ToString()
                                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(a => new ServiceAddOn
                                        {
                                            ServicePlusID = reader["ServicePlusID"]?.ToString(),
                                            ServicePlusName = a.Trim(),
                                            ServicePlusPrice = reader["ServicePlusPrice"] != DBNull.Value
                                                ? Convert.ToDecimal(reader["ServicePlusPrice"])
                                                : 0
                                        })
                                        .ToList(),

                                    ServiceFees = reader["AppointmentServiceFees"]?.ToString()
                                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(f => new ServiceFee
                                        {
                                            ServiceFeeID = reader["ServiceFeeID"]?.ToString(),
                                            ServiceFeeName = f.Trim(),
                                            ServiceFeePrice = reader["ServiceFeePrice"] != DBNull.Value
                                                ? Convert.ToDecimal(reader["ServiceFeePrice"])
                                                : 0
                                        })
                                        .ToList()
                                };

                                appointments.Add(appointment);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return appointments;
        }




        private void AddAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (DayViewHeader.Text.Contains("Appointments for"))
            {
                // Extract the selected date from the DayViewHeader
                DateTime selectedDate = DateTime.Parse(DayViewHeader.Text.Replace("Appointments for ", ""));
                var addAppointmentWindow = new AddAppointmentWindow(selectedDate);
                if (addAppointmentWindow.ShowDialog() == true)
                {
                    // Reload the appointments for the currently selected date
                    DayAppointmentsDataGrid.ItemsSource = LoadAppointments(selectedDate.Date);

                }
            }
            else
            {
                MessageBox.Show("No date selected. Please select a date first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CheckOutAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Appointment appointment)
            {
                // Open the TransactionLoginWindow to authenticate the employee
                var loginWindow = new TransactionLoginWindow();
                bool? loginResult = loginWindow.ShowDialog();

                if (loginResult == true)
                {
                    // Login successful, proceed with the transaction
                    string employeeID = loginWindow.EmployeeID;
                    string employeeName = loginWindow.EmployeeFirstName;

                    // Create a new instance of the PointOfSaleWindow with authenticated employee details
                    var posWindow = new PointOfSaleWindow_Release(employeeID, employeeName);

                    // Add services to the transaction
                    posWindow.AddServicesToTransaction(appointment);

                    // Show the PointOfSaleWindow
                    posWindow.Show();
                }
                else
                {
                    // Login failed or canceled
                    MessageBox.Show("Transaction login was not completed. Check out cannot proceed.",
                        "Transaction Canceled", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }


        public string CurrentMonth
        {
            get => currentMonth;
            private set
            {
                currentMonth = value;
                OnPropertyChanged(nameof(CurrentMonth));
            }
        }

        public string CurrentYear
        {
            get => currentYear;
            private set
            {
                currentYear = value;
                OnPropertyChanged(nameof(CurrentYear));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
