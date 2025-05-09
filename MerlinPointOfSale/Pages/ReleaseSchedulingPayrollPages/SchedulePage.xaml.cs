using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Media;

namespace MerlinPointOfSale.Pages.ReleaseSchedulingPayrollPages
{
    public partial class SchedulePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public SchedulePage()
        {
            InitializeComponent();
            PopulateWeekSelector();
            DisplayLocationInfo();
            LoadWeeklySchedule(DateTime.Today);
        }

        private void PopulateWeekSelector()
        {
            DateTime currentDate = DateTime.Today;
            DateTime startDate = currentDate.AddDays(-(int)currentDate.DayOfWeek).AddDays(-35); // 5 weeks ago
            DateTime endDate = currentDate.AddDays(-(int)currentDate.DayOfWeek).AddDays(35); // 5 weeks forward

            while (startDate <= endDate)
            {
                DateTime weekStart = startDate;
                DateTime weekEnd = startDate.AddDays(6);
                string displayText = $"{weekStart:MM/dd/yyyy} - {weekEnd:MM/dd/yyyy}";

                WeekSelector.Items.Add(new ComboBoxItem
                {
                    Content = displayText,
                    Tag = weekEnd // Store the week-ending date in the Tag
                });

                startDate = startDate.AddDays(7); // Move to the next week
            }

            WeekSelector.SelectedIndex = WeekSelector.Items.Count / 2; // Select the current week
        }

        private void DisplayLocationInfo()
        {
            string locationID = Properties.Settings.Default.LocationID;

            if (string.IsNullOrEmpty(locationID))
            {
                LocationTextBlock.Text = "Location ID: Not Set";
                LocationTotalHoursTextBlock.Text = "Total Scheduled Hours: 0.00 hours";
            }
            else
            {
                LocationTextBlock.Text = $"Location ID: {locationID}";
            }
        }

        private void LoadWeeklySchedule(DateTime weekEnding)
        {
            try
            {
                // Calculate the start of the week (Sunday) and dates for each day
                DateTime sunday = weekEnding.AddDays(-(int)weekEnding.DayOfWeek);
                var dayDates = Enumerable.Range(0, 7).Select(offset => sunday.AddDays(offset)).ToList();

                // Update DataGrid headers for each day
                ScheduleDataGrid.Columns[1].Header = $"Sunday ({dayDates[0]:MM/dd})";
                ScheduleDataGrid.Columns[2].Header = $"Monday ({dayDates[1]:MM/dd})";
                ScheduleDataGrid.Columns[3].Header = $"Tuesday ({dayDates[2]:MM/dd})";
                ScheduleDataGrid.Columns[4].Header = $"Wednesday ({dayDates[3]:MM/dd})";
                ScheduleDataGrid.Columns[5].Header = $"Thursday ({dayDates[4]:MM/dd})";
                ScheduleDataGrid.Columns[6].Header = $"Friday ({dayDates[5]:MM/dd})";
                ScheduleDataGrid.Columns[7].Header = $"Saturday ({dayDates[6]:MM/dd})";

                var schedule = new Dictionary<string, Dictionary<string, object>>();

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                SELECT 
                    S.EmployeeID,
                    RTRIM(E.EmployeeFirstName) + ' ' + RTRIM(E.EmployeeLastName) AS EmployeeName,
                    DATENAME(WEEKDAY, S.ShiftStartDate) AS DayName,
                    DATEDIFF(MINUTE, S.ShiftStartTime, S.ShiftEndTime) / 60.0 AS HoursWorked,
                    FORMAT(CAST(S.ShiftStartTime AS DATETIME), 'hh:mm tt') + ' - ' +
                    FORMAT(CAST(S.ShiftEndTime AS DATETIME), 'hh:mm tt') AS ShiftTime
                FROM 
                    LocationSchedule S
                JOIN 
                    Employees E ON S.EmployeeID = E.EmployeeID
                WHERE 
                    S.LocationID = @LocationID AND
                    S.ShiftStartDate BETWEEN @StartDate AND @EndDate
                ORDER BY 
                    E.EmployeeID, S.ShiftStartDate";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", Properties.Settings.Default.LocationID);
                        cmd.Parameters.AddWithValue("@StartDate", sunday);
                        cmd.Parameters.AddWithValue("@EndDate", dayDates.Last());

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string employeeID = reader["EmployeeID"].ToString();
                                string employeeName = reader["EmployeeName"].ToString();
                                string dayName = reader["DayName"].ToString();
                                string shiftTime = reader["ShiftTime"].ToString();
                                double hoursWorked = Convert.ToDouble(reader["HoursWorked"]);

                                // Ensure the employee exists in the schedule dictionary
                                if (!schedule.ContainsKey(employeeID))
                                {
                                    schedule[employeeID] = new Dictionary<string, object>
                            {
                                { "EmployeeName", employeeName },
                                { "Sunday", "" },
                                { "Monday", "" },
                                { "Tuesday", "" },
                                { "Wednesday", "" },
                                { "Thursday", "" },
                                { "Friday", "" },
                                { "Saturday", "" },
                                { "TotalHours", 0.0 }
                            };
                                }

                                // Add shift to the correct day and update total hours
                                var employeeData = schedule[employeeID];
                                if (employeeData.ContainsKey(dayName))
                                {
                                    employeeData[dayName] = shiftTime;
                                }
                                employeeData["TotalHours"] = (double)employeeData["TotalHours"] + hoursWorked;
                            }
                        }
                    }
                }

                // Transform the schedule dictionary into a list for DataGrid binding
                var scheduleData = schedule.Select(s => new
                {
                    EmployeeName = s.Value["EmployeeName"],
                    Sunday = s.Value["Sunday"],
                    Monday = s.Value["Monday"],
                    Tuesday = s.Value["Tuesday"],
                    Wednesday = s.Value["Wednesday"],
                    Thursday = s.Value["Thursday"],
                    Friday = s.Value["Friday"],
                    Saturday = s.Value["Saturday"],
                    TotalHours = s.Value["TotalHours"]
                }).ToList();

                ScheduleDataGrid.ItemsSource = scheduleData;

                // Update total scheduled hours display
                LocationTotalHoursTextBlock.Text = $"Total Scheduled Hours: {schedule.Values.Sum(emp => (double)emp["TotalHours"]):F2} hours";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void WeekSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WeekSelector.SelectedItem is ComboBoxItem selectedWeek)
            {
                DateTime weekEnding = (DateTime)selectedWeek.Tag;
                LoadWeeklySchedule(weekEnding);
            }
        }
        private void PrintScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            // Set print dialog properties
            printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
            printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.NorthAmericaLetter);

            if (printDialog.ShowDialog() == true)
            {
                // Clone the DataGrid for printing
                DataGrid printDataGrid = new DataGrid
                {
                    ItemsSource = ScheduleDataGrid.ItemsSource,
                    AutoGenerateColumns = false,
                    HeadersVisibility = DataGridHeadersVisibility.All,
                    RowStyle = ScheduleDataGrid.RowStyle,
                    ColumnHeaderStyle = ScheduleDataGrid.ColumnHeaderStyle,
                    Background = Brushes.White // Ensure it prints on white background
                };

                // Copy column definitions
                foreach (var column in ScheduleDataGrid.Columns)
                {
                    printDataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = column.Header,
                        Binding = ((DataGridTextColumn)column).Binding,
                        Width = column.Width
                    });
                }

                // Wrap the DataGrid in a container for printing
                FixedDocument fixedDoc = new FixedDocument();
                PageContent pageContent = new PageContent();
                FixedPage fixedPage = new FixedPage
                {
                    Width = printDialog.PrintableAreaWidth,
                    Height = printDialog.PrintableAreaHeight
                };

                // Add the DataGrid to the page
                printDataGrid.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                printDataGrid.Arrange(new Rect(new Point(0, 0), printDataGrid.DesiredSize));
                fixedPage.Children.Add(printDataGrid);

                // Add the page to the document
                ((IAddChild)pageContent).AddChild(fixedPage);
                fixedDoc.Pages.Add(pageContent);

                // Print the document
                printDialog.PrintDocument(fixedDoc.DocumentPaginator, "Schedule Print");
            }

        }
    }
}
