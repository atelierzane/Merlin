using System;
using System.Data.SqlClient;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Windows.DialogWindows;

namespace MerlinPointOfSale.Pages.ReleaseSchedulingPayrollPages
{
    public partial class TimePunchPage : Page
    {
        private DatabaseHelper databaseHelper = new DatabaseHelper();
        private System.Timers.Timer timer;
        private DateTime syncedTime;

        public TimePunchPage()
        {
            InitializeComponent();
            InitializeDateTimeDisplay();
        }

        private void InitializeDateTimeDisplay()
        {
            syncedTime = DateTimeHelper.GetNetworkTime();

            // Set the initial date
            CurrentDateTextBlock.Text = syncedTime.ToString("dddd, MMMM dd, yyyy");

            // Set up a high-precision timer to update time every second
            timer = new System.Timers.Timer(1000); // 1000ms = 1 second
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();

            // Sync the time periodically every minute (or a suitable interval)
            var syncTimer = new System.Timers.Timer(60000); // 60000ms = 1 minute
            syncTimer.Elapsed += SyncTimer_Elapsed;
            syncTimer.AutoReset = true;
            syncTimer.Start();

            // Initialize the time display
            UpdateTimeDisplay(syncedTime);
        }

        private void SyncTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            syncedTime = DateTimeHelper.GetNetworkTime();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Increment the synchronized time by 1 second
            syncedTime = syncedTime.AddSeconds(1);

            // Dispatch UI updates to the main thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update the date if it changes
                CurrentDateTextBlock.Text = syncedTime.ToString("dddd, MMMM dd, yyyy");

                // Update the time display
                UpdateTimeDisplay(syncedTime);
            });
        }


        private void UpdateTimeDisplay(DateTime currentTime)
        {
            // Convert to 12-hour format
            int hour = currentTime.Hour % 12;
            if (hour == 0) hour = 12; // Handle midnight and noon
            char[] hours = hour.ToString("D2").ToCharArray();

            char[] minutes = currentTime.Minute.ToString("D2").ToCharArray();
            char[] seconds = currentTime.Second.ToString("D2").ToCharArray();
            string amPm = currentTime.Hour < 12 ? "AM" : "PM";

            // Update and animate each digit
            AnimateDigitChange(HourTens, hours[0]);
            AnimateDigitChange(HourOnes, hours[1]);
            AnimateDigitChange(MinuteTens, minutes[0]);
            AnimateDigitChange(MinuteOnes, minutes[1]);
            AnimateDigitChange(SecondTens, seconds[0]);
            AnimateDigitChange(SecondOnes, seconds[1]);

            // Update the AM/PM display
            AmPmTextBlock.Text = amPm;
        }

        private void AnimateDigitChange(TextBlock textBlock, char newDigit)
        {
            if (textBlock.Text == newDigit.ToString())
                return; // Skip animation if the digit hasn't changed

            // Create a slide-out animation
            var slideOut = new ThicknessAnimation
            {
                From = new Thickness(0, 0, 0, 0),
                To = new Thickness(0, 20, 0, 0), // Move down
                Duration = TimeSpan.FromMilliseconds(125)
            };

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(125));

            slideOut.Completed += (s, e) =>
            {
                // Update the digit and create a slide-in animation
                textBlock.Text = newDigit.ToString();

                var slideIn = new ThicknessAnimation
                {
                    From = new Thickness(0, -20, 0, 0), // Start above the middle
                    To = new Thickness(0, 0, 0, 0), // Slide into position
                    Duration = TimeSpan.FromMilliseconds(125)
                };

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(125));

                // Begin slide-in and fade-in animations
                textBlock.BeginAnimation(MarginProperty, slideIn);
                textBlock.BeginAnimation(OpacityProperty, fadeIn);
            };

            // Begin slide-out and fade-out animations
            textBlock.BeginAnimation(MarginProperty, slideOut);
            textBlock.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void RecordTimePunch(string timePunchType)
        {
            var loginWindow = new EmployeeLoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                string employeeID = loginWindow.EmployeeID;
                string locationID = Properties.Settings.Default.LocationID;

                if (string.IsNullOrEmpty(locationID))
                {
                    MessageBox.Show("LocationID is not set in application settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                    {
                        conn.Open();

                        string query = @"
                            INSERT INTO LocationTimeCard 
                            (LocationID, EmployeeID, TimePunchDate, TimePunchTime, TimePunchType)
                            VALUES 
                            (@LocationID, @EmployeeID, @Date, @Time, @Type)";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@LocationID", locationID);
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                            cmd.Parameters.AddWithValue("@Date", DateTime.Now.Date);
                            cmd.Parameters.AddWithValue("@Time", DateTime.Now.TimeOfDay);
                            cmd.Parameters.AddWithValue("@Type", timePunchType);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show($"{timePunchType} recorded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error recording time punch: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClockInButton_Click(object sender, RoutedEventArgs e)
        {
            RecordTimePunch("Clock In");
        }

        private void ClockOutButton_Click(object sender, RoutedEventArgs e)
        {
            RecordTimePunch("Clock Out");
        }

        private void StartBreakButton_Click(object sender, RoutedEventArgs e)
        {
            RecordTimePunch("Start Break");
        }

        private void EndBreakButton_Click(object sender, RoutedEventArgs e)
        {
            RecordTimePunch("End Break");
        }
    }
}
