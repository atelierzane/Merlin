using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MerlinPointOfSale.Controls;
using static MerlinPointOfSale.Controls.ShiftDialog;


namespace MerlinPointOfSale.Controls
{
    public partial class WeeklyScheduleControl : UserControl
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private Dictionary<string, string> employees = new Dictionary<string, string>();
        private Dictionary<string, Grid> dayGrids;
        private Dictionary<Rectangle, string> shiftToEmployeeMap = new Dictionary<Rectangle, string>();
        private Dictionary<Rectangle, List<Tuple<Rectangle, TextBlock>>> shiftBreaksMap = new();
        private Dictionary<string, DateTime> dayDateMap = new Dictionary<string, DateTime>();
        private Rectangle activeRectangle;
        private Point initialMousePosition;
        private bool isDragging;
        private bool isResizingLeft;
        private bool isResizingRight;
        private const double DayWidth = 100; // Width of each day column
        private const double HourHeight = 50; // Height of each employee row
        private const double ResizeThreshold = 5; // Pixels near the edge for resizing
        private const double MinimumWidth = 10; // Minimum width of a shift rectangle


        public WeeklyScheduleControl()
        {
            InitializeComponent();
            InitializeDayGrids(); // Initialize dayGrids first
            PopulateWeekComboBox(); // Populate week dropdown
            string defaultLocationID = Properties.Settings.Default.LocationID;

            UpdateDayDates(DateTime.Today); // Initialize dayDateMap with today's week ending date


            // Force-load employees and shifts on startup for default view
            Loaded += (s, e) =>
            {
                string locationID = Properties.Settings.Default.LocationID;

                InitializeGridLines();

                if (WeekComboBox.Items.Count > 0 && WeekComboBox.SelectedItem == null)
                    WeekComboBox.SelectedIndex = WeekComboBox.Items.Count / 2;

                if (WeekComboBox.SelectedItem is ComboBoxItem selectedWeek)
                {
                    DateTime weekEnding = (DateTime)selectedWeek.Tag;
                    UpdateDayDates(weekEnding);
                    LoadAllocatedHours(locationID, weekEnding);

                    // Load all days of the week instead of just one
                    foreach (string day in new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" })
                    {
                        LoadEmployees(locationID, day); // Ensures employee rows are created
                        LoadShiftsForDay(day, locationID); // Ensures shift rectangles are drawn
                    }

                    // Default view
                    string selectedDay = subMenuBar.SelectedOption ?? "Sunday";
                    LoadDayGrid(selectedDay);
                }
            };


            DayContentGrid.SizeChanged += (s, e) =>
            {
                InitializeGridLines(); // Redraw grid lines when the size changes
            };

                    subMenuBar.SubMenuItems = new ObservableCollection<string>
            {
                "Sunday",
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday"
            };

            subMenuBar.ButtonClicked += SubMenuBar_ButtonClicked;
        }


        private void InitializeDayGrids()
        {
            dayGrids = new Dictionary<string, Grid>
            {
                { "Sunday", CreateDayGrid("Sunday") },
                { "Monday", CreateDayGrid("Monday") },
                { "Tuesday", CreateDayGrid("Tuesday") },
                { "Wednesday", CreateDayGrid("Wednesday") },
                { "Thursday", CreateDayGrid("Thursday") },
                { "Friday", CreateDayGrid("Friday") },
                { "Saturday", CreateDayGrid("Saturday") }
            };

            // Initially load the Sunday grid
            LoadDayGrid("Sunday");
        }
        private void UpdateDayDates(DateTime weekEnding)
        {
            // Always subtract 6 days from week ending to get the Sunday
            DateTime sunday = weekEnding.AddDays(-6);

            dayDateMap = new Dictionary<string, DateTime>
    {
        { "Sunday", sunday },
        { "Monday", sunday.AddDays(1) },
        { "Tuesday", sunday.AddDays(2) },
        { "Wednesday", sunday.AddDays(3) },
        { "Thursday", sunday.AddDays(4) },
        { "Friday", sunday.AddDays(5) },
        { "Saturday", sunday.AddDays(6) }
    };

            foreach (var day in dayGrids.Keys)
            {
                if (!dayDateMap.ContainsKey(day))
                {
                    MessageBox.Show($"Date for '{day}' is missing in dayDateMap.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                var dayGrid = dayGrids[day];
                var header = dayGrid.Children.OfType<TextBlock>().FirstOrDefault(h => h.Name == $"{day}Header");

                if (header != null)
                {
                    header.Text = $"{day} Schedule - {dayDateMap[day]:MM/dd/yyyy}";
                }
            }
        }



        private void SubMenuBar_ButtonClicked(object sender, string selectedOption)
        {
            // remember which day is currently active
            subMenuBar.SelectedOption = selectedOption;

            string selectedDay = selectedOption ?? "Sunday";

            // sanity-checks
            if (dayGrids == null || !dayGrids.ContainsKey(selectedDay))
            {
                MessageBox.Show(
                    $"Day '{selectedDay}' is not initialized in dayGrids.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (dayDateMap == null || !dayDateMap.ContainsKey(selectedDay))
            {
                MessageBox.Show(
                    $"Date for '{selectedDay}' is not initialized in dayDateMap.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // switch the view to the chosen day
            LoadDayGrid(selectedDay);

            string locationID = Properties.Settings.Default.LocationID;

            // refresh employees and shifts for that day
            LoadEmployees(locationID, selectedDay);
            LoadShiftsForDay(selectedDay, locationID);

            // update the running totals
            UpdateScheduledHours();
        }

        private void LoadShiftsForDay(string dayName, string locationID)
        {
            var canvas = GetCanvasContainer(dayName);
            if (canvas == null) return;

            canvas.Children.Clear();
            // purge stale rectangle references that just got cleared off the canvas
            shiftToEmployeeMap = shiftToEmployeeMap
                                    .Where(kvp => canvas.Children.Contains(kvp.Key))
                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            shiftBreaksMap = shiftBreaksMap
                                    .Where(kvp => canvas.Children.Contains(kvp.Key))
                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            DateTime shiftDate = dayDateMap[dayName];

            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            {
                conn.Open();
                string query = @"
            SELECT EmployeeID, ShiftStartTime, ShiftEndTime
            FROM LocationSchedule
            WHERE LocationID = @LocationID AND ShiftStartDate = @ShiftDate";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                    cmd.Parameters.AddWithValue("@ShiftDate", shiftDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var shiftData = new List<(string EmployeeID, double StartHour, double EndHour)>();

                        while (reader.Read())
                        {
                            string employeeID = reader["EmployeeID"].ToString();
                            double startHour = DateTime.Parse(reader["ShiftStartTime"].ToString()).TimeOfDay.TotalHours;
                            double endHour = DateTime.Parse(reader["ShiftEndTime"].ToString()).TimeOfDay.TotalHours;
                            shiftData.Add((employeeID, startHour, endHour));
                        }

                        // Use named handler to avoid duplicate registration
                        void Handler(object s, RoutedEventArgs e)
                        {
                            canvas.Loaded -= Handler;

                            foreach (var shift in shiftData)
                            {
                                CreateShiftRectangle(
                                    shift.EmployeeID,
                                    Array.IndexOf(new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" }, dayName),
                                    shift.StartHour,
                                    shift.EndHour
                                );
                            }
                        }

                        // Attach and immediately invoke if already loaded
                        canvas.Loaded += Handler;
                        if (canvas.IsLoaded)
                            Handler(canvas, new RoutedEventArgs());

                    }
                }
            }
        }



        private Grid CreateDayGrid(string dayName)
        {
            var grid = new Grid
            {
                Name = $"{dayName}Grid",
                Background = Brushes.Transparent,
                Margin = new Thickness(5)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) }); // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) }); // Time Labels
            grid.RowDefinitions.Add(new RowDefinition()); // Employee Rows

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Employee Names
            grid.ColumnDefinitions.Add(new ColumnDefinition()); // Schedule Canvas

            // Day Header
            var header = new TextBlock
            {
                Name = $"{dayName}Header", // Assign a unique name for each header
                Text = $"{dayName} Schedule",
                FontFamily = new FontFamily("Inter"),
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(header, 0);
            Grid.SetColumn(header, 1);
            grid.Children.Add(header);

            // Time Labels
            var timeGrid = new Grid
            {
                Background = Brushes.Transparent,
            };

            // Create 24 columns for 24 hours
            // Create 24 columns for 24 hours
            for (int hour = 0; hour < 24; hour++)
            {
                timeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var timeLabel = new TextBlock
                {
                    Text = FormatTime(hour), // Use FormatTime method to format as AM/PM
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontFamily = new FontFamily("Inter"),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                Grid.SetColumn(timeLabel, hour);
                timeGrid.Children.Add(timeLabel);
            }

            Grid.SetRow(timeGrid, 1);
            Grid.SetColumn(timeGrid, 1);
            grid.Children.Add(timeGrid);

            // Employee Grid
            var employeeGrid = new Grid { Name = $"{dayName}EmployeeGrid", Background = Brushes.Transparent };
            Grid.SetRow(employeeGrid, 2);
            Grid.SetColumn(employeeGrid, 0);
            grid.Children.Add(employeeGrid);

            // Schedule Canvas
            var canvas = new Canvas 
            { 
                Name = $"{dayName}Canvas", 
                Background = Brushes.Transparent,
                Margin = new Thickness(0)  
            };
            Grid.SetRow(canvas, 2);
            Grid.SetColumn(canvas, 1);
            grid.Children.Add(canvas);

            return grid;
        }
        private void LoadDayGrid(string dayName)
        {
            if (dayGrids == null || !dayGrids.ContainsKey(dayName))
            {
                MessageBox.Show($"Day grid for '{dayName}' is not available. Please check initialization.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DayContentGrid.Children.Clear(); // Clear existing content
            var dayGrid = dayGrids[dayName];
            DayContentGrid.Children.Add(dayGrid); // Add the selected day's grid
        }





        private void LoadEmployees(string locationID, string selectedDay)
        {
            try
            {
                employees.Clear();

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                SELECT EmployeeID, LTRIM(RTRIM(EmployeeFirstName)) + ' ' + LTRIM(RTRIM(EmployeeLastName)) AS EmployeeName
                FROM Employees
                WHERE EmployeePrimaryLocationID = @LocationID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees[reader["EmployeeID"].ToString().Trim()] = reader["EmployeeName"].ToString().Trim();
                            }
                        }
                    }
                }

                // Update the employee grid dynamically for the selected day
                if (!string.IsNullOrEmpty(selectedDay))
                {
                    var employeeGrid = GetEmployeeGrid(selectedDay);
                    if (employeeGrid != null)
                    {
                        employeeGrid.Children.Clear();
                        employeeGrid.RowDefinitions.Clear();

                        int row = 0;
                        foreach (var employee in employees.Values)
                        {
                            AddEmployeeLabel(employeeGrid, employee, row++);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSchedule(string dayName, string locationID)
        {
            try
            {
                var canvas = GetCanvasContainer(dayName);
                if (canvas == null) return;

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Delete existing shifts for this day and location to avoid duplicates
                    string deleteQuery = @"
                DELETE FROM LocationSchedule 
                WHERE LocationID = @LocationID AND (ShiftStartDay = @DayName OR ShiftEndDay = @DayName)";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@LocationID", locationID);
                        deleteCmd.Parameters.AddWithValue("@DayName", dayName);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Insert updated shifts
                    foreach (UIElement child in canvas.Children)
                    {
                        if (child is Rectangle shiftRectangle && shiftToEmployeeMap.TryGetValue(shiftRectangle, out string employeeID))
                        {
                            double shiftStartHour = Canvas.GetLeft(shiftRectangle) / canvas.ActualWidth * 24;
                            double shiftEndHour = (Canvas.GetLeft(shiftRectangle) + shiftRectangle.Width) / canvas.ActualWidth * 24;

                            DateTime shiftDate = dayDateMap[dayName];
                            TimeSpan shiftStartTime = TimeSpan.FromHours(shiftStartHour);
                            TimeSpan shiftEndTime = TimeSpan.FromHours(shiftEndHour);

                            string insertQuery = @"
                                                INSERT INTO LocationSchedule 
                                                (LocationID, EmployeeID, ShiftStartDate, ShiftStartTime, ShiftEndDate, ShiftEndTime, ShiftStartDay, ShiftEndDay)
                                                VALUES 
                                                (@LocationID, @EmployeeID, @ShiftStartDate, @ShiftStartTime, @ShiftEndDate, @ShiftEndTime, @ShiftStartDay, @ShiftEndDay)";

                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@LocationID", locationID);
                                insertCmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                                insertCmd.Parameters.AddWithValue("@ShiftStartDate", shiftDate);
                                insertCmd.Parameters.AddWithValue("@ShiftStartTime", shiftStartTime);
                                insertCmd.Parameters.AddWithValue("@ShiftEndDate", shiftDate); // same-day shift
                                insertCmd.Parameters.AddWithValue("@ShiftEndTime", shiftEndTime);
                                insertCmd.Parameters.AddWithValue("@ShiftStartDay", dayName);
                                insertCmd.Parameters.AddWithValue("@ShiftEndDay", dayName);

                                insertCmd.ExecuteNonQuery();
                            }

                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error saving schedule for {dayName}: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void AddEmployeeLabel(Grid employeeGrid, string employeeName, int row)
        {
            employeeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(HourHeight) });

            var label = new TextBlock
            {
                Text = employeeName,
                FontFamily = new FontFamily("Inter"),
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White, // Set text color to white
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Grid.SetRow(label, row);
            employeeGrid.Children.Add(label);
        }

        private void CreateShiftRectangle(string employeeID, int dayIndex, double startHour, double endHour)
        {
            var canvas = GetCanvasContainer(GetDayFromIndex(dayIndex));
            if (canvas == null) return;

            double timeSlotWidth = canvas.ActualWidth; // Account for employee column width
            double hourWidth = timeSlotWidth / 24; // Width for each hour

            // Snap start and end times to the nearest 15-minute intervals
            startHour = Math.Round(startHour * 4) / 4.0;
            endHour = Math.Round(endHour * 4) / 4.0;

            // Calculate the position and size of the shift rectangle
            double shiftLeft = startHour * hourWidth;
            double shiftWidth = (endHour - startHour) * hourWidth;
            double shiftTop = employees.Keys.ToList().IndexOf(employeeID) * HourHeight;

            if (shiftWidth < MinimumWidth)
            {
                MessageBox.Show("The shift duration is too short. Ensure the end time is greater than the start time.",
                                "Invalid Shift", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var shiftRectangle = new Rectangle
            {
                Width = shiftWidth,
                Height = HourHeight,
                Fill = Brushes.Cyan,
                Stroke = Brushes.White,
                StrokeThickness = 3,
            };

            var shiftLabel = new TextBlock
            {
                Text = FormatShiftDetails(startHour, endHour),
                FontFamily = new FontFamily("Inter"),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                Background = Brushes.Transparent,
                Padding = new Thickness(2),
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetLeft(shiftRectangle, shiftLeft );
            Canvas.SetTop(shiftRectangle, shiftTop - 5.5);

            Canvas.SetLeft(shiftLabel, shiftLeft);
            Canvas.SetTop(shiftLabel, shiftTop + 5);

            canvas.Children.Add(shiftRectangle);
            canvas.Children.Add(shiftLabel);

            shiftToEmployeeMap[shiftRectangle] = employeeID;

            AddHoverAnimation(shiftRectangle); // Add hover animation
            AddShiftHandlers(shiftRectangle, canvas, shiftLabel);
            AddContextMenuToShift(shiftRectangle, shiftLabel, canvas);

            // ✅ Ensure label includes break info and total time
            UpdateShiftLabelWithBreaks(shiftRectangle, shiftLabel);

            // ✅ Immediately update scheduled hours after full label is set
            UpdateScheduledHours();

        }

        private string FormatShiftDetails(double startHour, double endHour)
        {
            return $"{FormatTime(startHour)} - {FormatTime(endHour)}";
        }


        private string GetDayFromIndex(int dayIndex)
        {
            var days = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return dayIndex >= 0 && dayIndex < days.Length ? days[dayIndex] : null;
        }

        private void AddShiftHandlers(Rectangle rectangle, Canvas canvas, TextBlock label)
        {
            rectangle.MouseLeftButtonDown += (sender, e) =>
            {
                activeRectangle = rectangle;
                initialMousePosition = e.GetPosition(canvas);

                double mouseX = initialMousePosition.X;
                double rectLeft = Canvas.GetLeft(rectangle);
                double rectRight = rectLeft + rectangle.Width;

                isDragging = !(Math.Abs(mouseX - rectLeft) <= ResizeThreshold || Math.Abs(mouseX - rectRight) <= ResizeThreshold);
                isResizingLeft = Math.Abs(mouseX - rectLeft) <= ResizeThreshold;
                isResizingRight = Math.Abs(mouseX - rectRight) <= ResizeThreshold;

                rectangle.CaptureMouse();
                e.Handled = true;
            };

            rectangle.MouseMove += (sender, e) =>
            {
                if (activeRectangle == null || e.LeftButton != MouseButtonState.Pressed) return;

                Point currentMousePosition = e.GetPosition(canvas);
                double offsetX = currentMousePosition.X - initialMousePosition.X;

                if (isDragging)
                {
                    double newLeft = Canvas.GetLeft(activeRectangle) + offsetX;

                    if (newLeft >= 0 && newLeft + activeRectangle.Width <= canvas.ActualWidth)
                    {
                        Canvas.SetLeft(activeRectangle, newLeft);

                        // Update the shift label's position and text
                        Canvas.SetLeft(label, newLeft);
                        UpdateShiftLabelWithBreaks(activeRectangle, label);

                        // Update the positions of associated break rectangles
                        if (shiftBreaksMap.ContainsKey(activeRectangle))
                        {
                            foreach (var breakPair in shiftBreaksMap[activeRectangle])
                            {
                                var breakRectangle = breakPair.Item1;
                                var breakLabel = breakPair.Item2;

                                // Calculate the new position for the break rectangle
                                double breakOffset = offsetX;
                                Canvas.SetLeft(breakRectangle, Canvas.GetLeft(breakRectangle) + breakOffset);

                                // Update the break label's position
                                UpdateBreakLabel(breakRectangle, breakLabel, canvas);
                            }
                        }

                        // Update scheduled hours
                        UpdateScheduledHours();
                        initialMousePosition = currentMousePosition;
                    }
                }
                else if (isResizingLeft)
                {
                    double newLeft = Canvas.GetLeft(activeRectangle) + offsetX;
                    double newWidth = activeRectangle.Width - offsetX;

                    if (newWidth >= MinimumWidth && newLeft >= 0)
                    {
                        Canvas.SetLeft(activeRectangle, newLeft);
                        activeRectangle.Width = newWidth;

                        // Update the shift label's position and text
                        Canvas.SetLeft(label, newLeft);
                        UpdateShiftLabelWithBreaks(activeRectangle, label);

                        // Update positions of associated breaks if resizing changes the start time
                        UpdateBreakPositionsOnResize(activeRectangle, canvas);

                        // Update scheduled hours
                        UpdateScheduledHours();
                        initialMousePosition = currentMousePosition;
                    }
                }
                else if (isResizingRight)
                {
                    double newWidth = activeRectangle.Width + offsetX;

                    if (newWidth >= MinimumWidth && Canvas.GetLeft(activeRectangle) + newWidth <= canvas.ActualWidth)
                    {
                        activeRectangle.Width = newWidth;

                        // Update the shift label's position and text
                        UpdateShiftLabelWithBreaks(activeRectangle, label);

                        // Update positions of associated breaks if resizing changes the end time
                        UpdateBreakPositionsOnResize(activeRectangle, canvas);

                        // Update scheduled hours
                        UpdateScheduledHours();
                        initialMousePosition = currentMousePosition;
                    }
                }
            };


            rectangle.MouseLeftButtonUp += (sender, e) =>
            {
                if (activeRectangle != null)
                {
                    activeRectangle.ReleaseMouseCapture();
                    activeRectangle = null;
                    isDragging = false;
                    isResizingLeft = false;
                    isResizingRight = false;

                    // Final update to ensure everything is accurate
                    UpdateShiftLabelWithBreaks(rectangle, label);
                    // Update scheduled hours
                    UpdateScheduledHours();

                    // Update all associated break labels to ensure they are accurate
                    if (shiftBreaksMap.ContainsKey(rectangle))
                    {
                        foreach (var breakPair in shiftBreaksMap[rectangle])
                        {
                            var breakRectangle = breakPair.Item1;
                            var breakLabel = breakPair.Item2;

                            UpdateBreakLabel(breakRectangle, breakLabel, canvas);
                            // Update scheduled hours
                            UpdateScheduledHours();
                        }
                    }
                }
            };

            rectangle.MouseEnter += (sender, e) =>
            {
                // Change the cursor when hovering near the edges
                double mouseX = e.GetPosition(rectangle).X;

                if (Math.Abs(mouseX) <= ResizeThreshold || Math.Abs(mouseX - rectangle.Width) <= ResizeThreshold)
                {
                    rectangle.Cursor = Cursors.SizeWE;
                }
                else
                {
                    rectangle.Cursor = Cursors.Hand;
                }
            };

            rectangle.MouseLeave += (sender, e) =>
            {
                rectangle.Cursor = Cursors.Arrow;
            };
        }

        private void UpdateBreakPositionsOnResize(Rectangle shiftRectangle, Canvas canvas)
        {
            if (!shiftBreaksMap.ContainsKey(shiftRectangle)) return;

            foreach (var breakPair in shiftBreaksMap[shiftRectangle])
            {
                var breakRectangle = breakPair.Item1;
                var breakLabel = breakPair.Item2;

                // Ensure the break rectangle stays within the resized shift bounds
                double shiftStart = Canvas.GetLeft(shiftRectangle);
                double shiftEnd = shiftStart + shiftRectangle.Width;

                double breakLeft = Canvas.GetLeft(breakRectangle);
                double breakRight = breakLeft + breakRectangle.Width;

                if (breakLeft < shiftStart)
                {
                    Canvas.SetLeft(breakRectangle, shiftStart);
                    breakRectangle.Width = Math.Max(MinimumWidth, breakRight - shiftStart);
                }
                else if (breakRight > shiftEnd)
                {
                    breakRectangle.Width = Math.Max(MinimumWidth, shiftEnd - breakLeft);
                }

                // Update the break label's position and text
                UpdateBreakLabel(breakRectangle, breakLabel, canvas);
            }
        }

        private void UpdateLabel(Rectangle rectangle, TextBlock label)
        {
            if (rectangle.Parent is Canvas canvas)
            {
                double left = Canvas.GetLeft(rectangle);
                double right = left + rectangle.Width;

                double startHour = (left / canvas.ActualWidth) * 24;
                double endHour = (right / canvas.ActualWidth) * 24;

                label.Text = FormatShiftDetails(startHour, endHour);
                Canvas.SetLeft(label, left); // Update label position
            }
        }



        private void AddShiftButton_Click(object sender, RoutedEventArgs e)
        {
            string locationID = Properties.Settings.Default.LocationID;


                string initialDay = subMenuBar.SelectedOption ?? "Sunday";

                var shiftDialog = new ShiftDialog(employees, initialDay); // Pass employees and initial day
                if (shiftDialog.ShowDialog() == true)
                {
                    string employeeID = shiftDialog.SelectedEmployeeID;
                    string selectedDay = shiftDialog.SelectedDay;
                    int dayIndex = Array.IndexOf(new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" }, selectedDay);

                    // Add the shift to the selected day
                    CreateShiftRectangle(employeeID, dayIndex, shiftDialog.StartTime.Hour, shiftDialog.EndTime.Hour);
                }
            
            else
            {
                MessageBox.Show("Please select a location and ensure employees are loaded before adding a shift.", "Add Shift", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }




        private void AddBreakHandlers(Rectangle breakRectangle, Canvas canvas, Rectangle parentShiftRectangle, TextBlock breakLabel)
        {
            breakRectangle.MouseLeftButtonDown += (sender, e) =>
            {
                activeRectangle = breakRectangle;
                initialMousePosition = e.GetPosition(canvas);

                double mouseX = initialMousePosition.X;
                double rectLeft = Canvas.GetLeft(breakRectangle);
                double rectRight = rectLeft + breakRectangle.Width;

                isDragging = !(Math.Abs(mouseX - rectLeft) <= ResizeThreshold || Math.Abs(mouseX - rectRight) <= ResizeThreshold);
                isResizingLeft = Math.Abs(mouseX - rectLeft) <= ResizeThreshold;
                isResizingRight = Math.Abs(mouseX - rectRight) <= ResizeThreshold;

                breakRectangle.CaptureMouse();
                e.Handled = true;
            };

            breakRectangle.MouseMove += (sender, e) =>
            {
                if (activeRectangle == null || e.LeftButton != MouseButtonState.Pressed) return;

                Point currentMousePosition = e.GetPosition(canvas);
                double offsetX = currentMousePosition.X - initialMousePosition.X;

                double parentLeft = Canvas.GetLeft(parentShiftRectangle);
                double parentRight = parentLeft + parentShiftRectangle.Width;

                if (isResizingLeft)
                {
                    double newLeft = Canvas.GetLeft(breakRectangle) + offsetX;
                    double newWidth = breakRectangle.Width - offsetX;

                    // Ensure resizing doesn't exceed shift bounds
                    if (newLeft >= parentLeft && newWidth >= MinimumWidth)
                    {
                        Canvas.SetLeft(breakRectangle, newLeft);
                        breakRectangle.Width = newWidth;

                        // Update break details and shift label
                        UpdateBreakLabel(breakRectangle, breakLabel, canvas);
                        UpdateShiftLabelWithBreaks(parentShiftRectangle, GetShiftLabel(parentShiftRectangle, canvas));
                        initialMousePosition = currentMousePosition;
                        UpdateScheduledHours();
                    }
                }
                else if (isResizingRight)
                {
                    double newWidth = breakRectangle.Width + offsetX;

                    // Ensure resizing doesn't exceed shift bounds
                    if (Canvas.GetLeft(breakRectangle) + newWidth <= parentRight && newWidth >= MinimumWidth)
                    {
                        breakRectangle.Width = newWidth;

                        // Update break details and shift label
                        UpdateBreakLabel(breakRectangle, breakLabel, canvas);
                        UpdateShiftLabelWithBreaks(parentShiftRectangle, GetShiftLabel(parentShiftRectangle, canvas));
                        initialMousePosition = currentMousePosition;
                        UpdateScheduledHours();
                    }
                }
                else if (isDragging)
                {
                    double newLeft = Canvas.GetLeft(breakRectangle) + offsetX;

                    // Ensure dragging stays within shift bounds
                    if (newLeft >= parentLeft && newLeft + breakRectangle.Width <= parentRight)
                    {
                        Canvas.SetLeft(breakRectangle, newLeft);

                        // Update break details and shift label
                        UpdateBreakLabel(breakRectangle, breakLabel, canvas);
                        UpdateShiftLabelWithBreaks(parentShiftRectangle, GetShiftLabel(parentShiftRectangle, canvas));
                        initialMousePosition = currentMousePosition;
                    }
                }
            };

            breakRectangle.MouseLeftButtonUp += (sender, e) =>
            {
                if (activeRectangle != null)
                {
                    activeRectangle.ReleaseMouseCapture();
                    activeRectangle = null;
                    isDragging = false;
                    isResizingLeft = false;
                    isResizingRight = false;

                    // Final update to ensure everything is accurate
                    UpdateBreakLabel(breakRectangle, breakLabel, canvas);
                    UpdateShiftLabelWithBreaks(parentShiftRectangle, GetShiftLabel(parentShiftRectangle, canvas));
                }
            };
        }
        private TextBlock GetShiftLabel(Rectangle shiftRectangle, Canvas canvas)
        {
            foreach (var child in canvas.Children)
            {
                if (child is TextBlock textBlock && Canvas.GetLeft(textBlock) >= Canvas.GetLeft(shiftRectangle))
                {
                    return textBlock; // Return the first matching label near the rectangle
                }
            }
            return null;
        }


        private void UpdateBreakLabel(Rectangle breakRectangle, TextBlock breakLabel, Canvas canvas)
        {
            double breakStartHour = (Canvas.GetLeft(breakRectangle) / canvas.ActualWidth) * 24;
            double breakEndHour = ((Canvas.GetLeft(breakRectangle) + breakRectangle.Width) / canvas.ActualWidth) * 24;

            // Update the break label's text
            breakLabel.Text = FormatBreakDetails(breakStartHour, breakEndHour);

            // Update the break label's position
            Canvas.SetLeft(breakLabel, Canvas.GetLeft(breakRectangle));
        }




        private void WeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string locationID = Properties.Settings.Default.LocationID;

            if (WeekComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is DateTime weekEnding)
            {
                string selectedDay = subMenuBar.SelectedOption ?? "Sunday";
                

                UpdateDayDates(weekEnding);
                LoadEmployees(locationID, selectedDay);
                LoadAllocatedHours(locationID, weekEnding);
                UpdateScheduledHours();
            }


        }




        private Grid GetEmployeeGrid(string dayName)
        {
            if (dayGrids.ContainsKey(dayName))
            {
                var dayGrid = dayGrids[dayName];
                return dayGrid.Children.OfType<Grid>().FirstOrDefault(g => g.Name == $"{dayName}EmployeeGrid");
            }
            return null;
        }

        private Canvas GetCanvasContainer(string dayName)
        {
            if (dayGrids.ContainsKey(dayName))
            {
                var dayGrid = dayGrids[dayName];
                return dayGrid.Children.OfType<Canvas>().FirstOrDefault(c => c.Name == $"{dayName}Canvas");
            }
            return null;
        }



        private void PopulateWeekComboBox()
        {
            WeekComboBox.Items.Clear();

            DateTime currentDate = DateTime.Today;
            DateTime startDate = currentDate.AddDays(-(int)currentDate.DayOfWeek).AddDays(-35); // 5 weeks ago
            DateTime endDate = currentDate.AddDays(-(int)currentDate.DayOfWeek).AddDays(35); // 5 weeks forward

            while (startDate <= endDate)
            {
                DateTime weekStart = startDate;
                DateTime weekEnd = startDate.AddDays(6);
                string displayText = $"{weekStart:MM/dd/yyyy} - {weekEnd:MM/dd/yyyy}";

                WeekComboBox.Items.Add(new ComboBoxItem
                {
                    Content = displayText,
                    Tag = weekEnd // Store the week-ending date in the Tag
                });

                startDate = startDate.AddDays(7); // Move to the next week
            }

            WeekComboBox.SelectedIndex = WeekComboBox.Items.Count / 2;
        }
        private void AddContextMenuToShift(Rectangle shiftRectangle, TextBlock label, Canvas canvas)
        {
            var contextMenu = new ContextMenu();

            // Add Break Option
            var addBreakMenuItem = new MenuItem { Header = "Add Break" };
            addBreakMenuItem.Click += (sender, e) => ShowBreakDialog(shiftRectangle, label, canvas);

            // Remove All Breaks Option
            var removeBreaksMenuItem = new MenuItem { Header = "Remove All Breaks" };
            removeBreaksMenuItem.Click += (sender, e) => RemoveAllBreaks(shiftRectangle, label);

            // Remove Shift Option
            var removeShiftMenuItem = new MenuItem { Header = "Remove Shift" };
            removeShiftMenuItem.Click += (sender, e) => RemoveShift(shiftRectangle, label, canvas);
            // Change Shift Time Option
            var changeShiftTimeMenuItem = new MenuItem { Header = "Change Shift Time" };
            changeShiftTimeMenuItem.Click += (sender, e) => ShowChangeShiftTimeDialog(shiftRectangle, label, canvas);


            // Add items to the context menu
            contextMenu.Items.Add(addBreakMenuItem);
            contextMenu.Items.Add(changeShiftTimeMenuItem);
            contextMenu.Items.Add(removeBreaksMenuItem);
            contextMenu.Items.Add(removeShiftMenuItem);

            // Attach the context menu to the shift rectangle
            shiftRectangle.ContextMenu = contextMenu;
        }

        private void ShowChangeShiftTimeDialog(Rectangle shiftRectangle, TextBlock label, Canvas canvas)
        {
            double canvasWidth = canvas.ActualWidth;
            double shiftStartHour = (Canvas.GetLeft(shiftRectangle) / canvasWidth) * 24;
            double shiftEndHour = ((Canvas.GetLeft(shiftRectangle) + shiftRectangle.Width) / canvasWidth) * 24;

            var shiftDialog = new ShiftDialog(employees, subMenuBar.SelectedOption ?? "Sunday")
            {
                StartTime = DateTime.Today.Add(TimeSpan.FromHours(shiftStartHour)),
                EndTime = DateTime.Today.Add(TimeSpan.FromHours(shiftEndHour))
            };

            if (shiftDialog.ShowDialog() == true)
            {
                double newStartHour = shiftDialog.StartTime.TimeOfDay.TotalHours;
                double newEndHour = shiftDialog.EndTime.TimeOfDay.TotalHours;

                if (newEndHour > newStartHour)
                {
                    // Update rectangle position and size
                    double newLeft = (newStartHour / 24) * canvasWidth;
                    double newWidth = ((newEndHour - newStartHour) / 24) * canvasWidth;

                    Canvas.SetLeft(shiftRectangle, newLeft);
                    shiftRectangle.Width = newWidth;

                    // Update the label
                    Canvas.SetLeft(label, newLeft);
                    UpdateShiftLabelWithBreaks(shiftRectangle, label);

                    // Update associated breaks, if any
                    UpdateBreakPositionsOnResize(shiftRectangle, canvas);

                    // Update the schedule
                    UpdateScheduledHours();

                    MessageBox.Show("Shift time updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("End time must be after start time.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void RemoveShift(Rectangle shiftRectangle, TextBlock label, Canvas canvas)
        {
            // Remove associated breaks
            if (shiftBreaksMap.ContainsKey(shiftRectangle))
            {
                foreach (var breakPair in shiftBreaksMap[shiftRectangle])
                {
                    var breakRectangle = breakPair.Item1;
                    var breakLabel = breakPair.Item2;

                    // Remove the break rectangle and label from the canvas
                    if (breakRectangle.Parent is Canvas)
                    {
                        canvas.Children.Remove(breakRectangle);
                        canvas.Children.Remove(breakLabel);
                    }
                }

                // Clear the breaks from the shiftBreaksMap
                shiftBreaksMap.Remove(shiftRectangle);
            }

            // Remove the shift rectangle and its label
            if (shiftRectangle.Parent is Canvas)
            {
                canvas.Children.Remove(shiftRectangle);
                canvas.Children.Remove(label);
            }

            // Remove the shift from the shiftToEmployeeMap
            if (shiftToEmployeeMap.ContainsKey(shiftRectangle))
            {
                shiftToEmployeeMap.Remove(shiftRectangle);
            }

            // Save the updated schedule
            string dayName = subMenuBar.SelectedOption ?? "Sunday"; // Get the currently selected day
            string locationID = Properties.Settings.Default.LocationID;

            
        
                SaveSchedule(dayName, locationID); // Save the updated schedule to the database
            
            UpdateScheduledHours();
        }


        private void ShowBreakDialog(Rectangle shiftRectangle, TextBlock label, Canvas canvas)
        {
            var breakDialog = new BreakDialog();
            if (breakDialog.ShowDialog() == true)
            {
                double shiftStartHour = Canvas.GetLeft(shiftRectangle) / canvas.ActualWidth * 24;
                double shiftEndHour = (Canvas.GetLeft(shiftRectangle) + shiftRectangle.Width) / canvas.ActualWidth * 24;

                double breakStartHour = breakDialog.BreakStart.TimeOfDay.TotalHours;
                double breakEndHour = breakDialog.BreakEnd.TimeOfDay.TotalHours;

                // Validate Break Times
                if (breakStartHour >= shiftStartHour && breakEndHour <= shiftEndHour && breakStartHour < breakEndHour)
                {
                    CreateBreakRectangle(shiftRectangle, label, canvas, breakStartHour, breakEndHour);
                }
                else
                {
                    MessageBox.Show("Break must be within the shift's time range and valid.", "Invalid Break", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void CreateBreakRectangle(Rectangle shiftRectangle, TextBlock label, Canvas canvas, double breakStartHour, double breakEndHour)
        {
            double canvasWidth = canvas.ActualWidth;
            double breakLeft = (breakStartHour / 24) * canvasWidth;
            double breakWidth = ((breakEndHour - breakStartHour) / 24) * canvasWidth;

            var breakRectangle = new Rectangle
            {
                Width = breakWidth,
                Height = HourHeight,
                Fill = Brushes.White,
                Stroke = Brushes.Gray,
                StrokeThickness = 3,
                Margin = new Thickness(0, 0, 0, 0),
            };

            var breakLabel = new TextBlock
            {
                Text = FormatBreakDetails(breakStartHour, breakEndHour),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Inter"),
                Foreground = Brushes.Black,
                Background = Brushes.Transparent,
                TextAlignment = TextAlignment.Center
            };

            double shiftTop = Canvas.GetTop(shiftRectangle) + HourHeight / 4;
            Canvas.SetLeft(breakRectangle, breakLeft);
            Canvas.SetTop(breakRectangle, shiftTop - 12);

            Canvas.SetLeft(breakLabel, breakLeft);
            Canvas.SetTop(breakLabel, shiftTop + 5);

            canvas.Children.Add(breakRectangle);
            canvas.Children.Add(breakLabel);

            if (!shiftBreaksMap.ContainsKey(shiftRectangle))
                shiftBreaksMap[shiftRectangle] = new List<Tuple<Rectangle, TextBlock>>();

            shiftBreaksMap[shiftRectangle].Add(Tuple.Create(breakRectangle, breakLabel));

            AddHoverAnimation(breakRectangle); // Add hover animation
            AddBreakHandlers(breakRectangle, canvas, shiftRectangle, breakLabel);
            UpdateShiftLabelWithBreaks(shiftRectangle, label);

            UpdateScheduledHours();
        }

        private void AddHoverAnimation(Rectangle rectangle)
        {
            // Create a ScaleTransform and set it as the rectangle's RenderTransform
            var scaleTransform = new ScaleTransform(1.0, 1.0);
            rectangle.RenderTransform = scaleTransform;
            rectangle.RenderTransformOrigin = new Point(0.5, 0.5); // Scale from the center

            // Define animations for scale
            var scaleUpAnimationX = new DoubleAnimation
            {
                To = 1.035, // Scale up to 105% width
                Duration = TimeSpan.FromMilliseconds(65)
            };

            var scaleUpAnimationY = new DoubleAnimation
            {
                To = 1.035, // Scale up to 105% height
                Duration = TimeSpan.FromMilliseconds(65)
            };

            var scaleDownAnimationX = new DoubleAnimation
            {
                To = 1.0, // Scale back to original size
                Duration = TimeSpan.FromMilliseconds(85)
            };

            var scaleDownAnimationY = new DoubleAnimation
            {
                To = 1.0, // Scale back to original size
                Duration = TimeSpan.FromMilliseconds(85)
            };

            // Attach hover events
            rectangle.MouseEnter += (s, e) =>
            {
                // Begin scale-up animations
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUpAnimationX);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUpAnimationY);
            };

            rectangle.MouseLeave += (s, e) =>
            {
                // Begin scale-down animations
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDownAnimationX);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDownAnimationY);
            };
        }



        private void RemoveAllBreaks(Rectangle shiftRectangle, TextBlock label)
        {
            if (shiftBreaksMap.ContainsKey(shiftRectangle))
            {
                foreach (var breakPair in shiftBreaksMap[shiftRectangle])
                {
                    var breakRectangle = breakPair.Item1;
                    var breakLabel = breakPair.Item2;

                    if (breakRectangle.Parent is Canvas canvas)
                    {
                        canvas.Children.Remove(breakRectangle);
                        canvas.Children.Remove(breakLabel);
                    }
                }

                shiftBreaksMap[shiftRectangle].Clear();
                UpdateShiftLabelWithBreaks(shiftRectangle, label);
                UpdateScheduledHours();
            }
        }
        private void UpdateShiftLabelWithBreaks(Rectangle shiftRectangle, TextBlock shiftLabel)
        {
            if (shiftRectangle.Parent is Canvas canvas)
            {
                double shiftStart = Canvas.GetLeft(shiftRectangle);
                double shiftEnd = shiftStart + shiftRectangle.Width;
                double shiftStartHour = (shiftStart / canvas.ActualWidth) * 24;
                double shiftEndHour = (shiftEnd / canvas.ActualWidth) * 24;

                double totalBreakTime = 0;

                string shiftDetails = FormatShiftDetails(shiftStartHour, shiftEndHour);

                if (shiftBreaksMap.ContainsKey(shiftRectangle) && shiftBreaksMap[shiftRectangle].Count > 0)
                {
                    var breaks = shiftBreaksMap[shiftRectangle]
                        .Select(bp =>
                        {
                            var breakRectangle = bp.Item1;
                            if (breakRectangle.Parent is Canvas breakCanvas)
                            {
                                double breakStartHour = (Canvas.GetLeft(breakRectangle) / breakCanvas.ActualWidth) * 24;
                                double breakEndHour = ((Canvas.GetLeft(breakRectangle) + breakRectangle.Width) / breakCanvas.ActualWidth) * 24;

                                totalBreakTime += breakEndHour - breakStartHour;

                                return $"{FormatTime(breakStartHour)} - {FormatTime(breakEndHour)}";
                            }
                            return string.Empty;
                        })
                        .Where(b => !string.IsNullOrEmpty(b))
                        .ToList();

                    shiftDetails += "\nBreaks:\n" + string.Join("\n", breaks);
                }

                TimeSpan shiftDuration = TimeSpan.FromHours(shiftEndHour - shiftStartHour - totalBreakTime);
                shiftDetails += $"\nTotal Time: {shiftDuration.Hours}h {shiftDuration.Minutes}m";

                shiftLabel.Text = shiftDetails;
            }
        }
        private void UpdateScheduledHours()
        {
            try
            {
                double totalHours = 0;
                string locationID = Properties.Settings.Default.LocationID;

                if (WeekComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is DateTime weekEnding)
                {
                    DateTime weekStart = weekEnding.AddDays(-(int)weekEnding.DayOfWeek); // Get Sunday

                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();

                        string query = @"
                    SELECT 
                        SUM(
                            DATEDIFF(MINUTE, ShiftStartTime, ShiftEndTime) / 60.0 
                            - ISNULL(ShiftBreakTotal, 0)
                        ) AS TotalHours
                    FROM LocationSchedule
                    WHERE LocationID = @LocationID 
                      AND ShiftStartDate BETWEEN @WeekStart AND @WeekEnd";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@LocationID", locationID);
                            cmd.Parameters.AddWithValue("@WeekStart", weekStart.Date);
                            cmd.Parameters.AddWithValue("@WeekEnd", weekStart.AddDays(6).Date);

                            object result = cmd.ExecuteScalar();
                            if (result != DBNull.Value)
                            {
                                totalHours = Convert.ToDouble(result);
                            }
                        }
                    }
                }

                ScheduledHoursTextBlock.Text = $"Scheduled Hours: {totalHours:F2}h";

                // Compare with allocated hours
                var allocatedText = AllocatedHoursTextBlock.Text;
                var match = Regex.Match(allocatedText, @"\d+(\.\d+)?");
                double allocated = match.Success ? double.Parse(match.Value) : 0;

                ScheduledHoursTextBlock.Foreground = totalHours > allocated ? Brushes.Red : Brushes.White;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving scheduled hours: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void InitializeGridLines()
        {
            GridLinesCanvas.Children.Clear();

            // Ensure the canvas dimensions are properly set
            GridLinesCanvas.Width = DayContentGrid.ActualWidth > 0 ? DayContentGrid.ActualWidth : 800;
            GridLinesCanvas.Height = DayContentGrid.ActualHeight > 0 ? DayContentGrid.ActualHeight : 600;

            double canvasWidth = GridLinesCanvas.Width;

            // Define the width of the employee names column (matches your XAML definition)
            const double EmployeeColumnWidth = 200;

            // Calculate the width available for the time slots
            double timeSlotWidth = canvasWidth - EmployeeColumnWidth;

            // Match the number of hours with the TimeGrid (24 columns for 24 hours)
            double hourWidth = timeSlotWidth / 24;
            double intervalWidth = hourWidth / 4;

            // Define starting height to leave space for headers and time labels
            const double HeaderHeight = 60;

            // Dynamically calculate the height of the vertical lines
            double gridHeight = HeaderHeight + (employees.Count * HourHeight);

            // Draw vertical lines for each hour
            for (int hour = 0; hour <= 24; hour++)
            {
                double x = EmployeeColumnWidth + (hour * hourWidth);

                // Vertical line for the hour
                var hourLine = new Line
                {
                    X1 = x,
                    Y1 = HeaderHeight,
                    X2 = x,
                    Y2 = gridHeight,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5
                };
                GridLinesCanvas.Children.Add(hourLine);

                // Draw thinner lines for 15-minute intervals
                if (hour < 24)
                {
                    for (int quarter = 1; quarter <= 3; quarter++)
                    {
                        double quarterX = x + (quarter * intervalWidth);
                        var quarterLine = new Line
                        {
                            X1 = quarterX,
                            Y1 = HeaderHeight,
                            X2 = quarterX,
                            Y2 = gridHeight,
                            Stroke = Brushes.Gray,
                            StrokeThickness = 0.5
                        };
                        GridLinesCanvas.Children.Add(quarterLine);
                    }
                }
            }

            // Draw horizontal lines for rows (e.g., employees)
            for (int i = 0; i <= employees.Count; i++)
            {
                double y = HeaderHeight + (i * HourHeight);
                var rowLine = new Line
                {
                    X1 = EmployeeColumnWidth,
                    Y1 = y,
                    X2 = canvasWidth,
                    Y2 = y,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                GridLinesCanvas.Children.Add(rowLine);
            }
        }



        private void DayContentGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitializeGridLines();
        }

        private string FormatBreakDetails(double startHour, double endHour)
        {
            return $"{FormatTime(startHour)} - {FormatTime(endHour)}";
        }


        private string FormatTime(double hour)
        {
            int totalHours = (int)Math.Floor(hour);
            int minutes = (int)((hour - totalHours) * 60);
            DateTime time = DateTime.Today.AddHours(totalHours).AddMinutes(minutes);
            return time.ToString("hh:mm"); // 12-hour format with AM/PM
        }



        private void LoadAllocatedHours(string locationID, DateTime weekEnding)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        SELECT LocationPayrollHoursAllocated 
                        FROM LocationPayroll
                        WHERE LocationID = @LocationID AND LocationPayrollWeekEnding = @WeekEnding";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);
                        cmd.Parameters.AddWithValue("@WeekEnding", weekEnding.Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                AllocatedHoursTextBlock.Text = $"Allocated Hours: {reader["LocationPayrollHoursAllocated"]}h";
                            }
                            else
                            {
                                AllocatedHoursTextBlock.Text = "Allocated Hours: 0h";
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading allocated hours: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PostScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            string locationID = Properties.Settings.Default.LocationID;

            try
            {
                using var conn = new SqlConnection(dbHelper.GetConnectionString());
                conn.Open();

                foreach (var kvp in dayDateMap)           // Sunday-Saturday
                {
                    string dayName = kvp.Key;
                    DateTime shiftDate = kvp.Value;

                    var canvas = GetCanvasContainer(dayName);
                    if (canvas == null) continue;

                    //  do we have at least one shift rectangle for this day?
                    bool hasShifts = canvas.Children
                                           .OfType<Rectangle>()
                                           .Any(r => shiftToEmployeeMap.ContainsKey(r));
                    if (!hasShifts) continue;             // nothing to post

                    // 1) wipe the rows for this date
                    const string wipeSql =
                        @"DELETE FROM LocationSchedule
                  WHERE LocationID     = @LID
                    AND ShiftStartDate = @SDate;";

                    using (var wipeCmd = new SqlCommand(wipeSql, conn))
                    {
                        wipeCmd.Parameters.AddWithValue("@LID", locationID);
                        wipeCmd.Parameters.AddWithValue("@SDate", shiftDate.Date);
                        wipeCmd.ExecuteNonQuery();
                    }

                    // 2) write what’s on the canvas
                    SaveDaySchedule(dayName, locationID, conn);
                }

                MessageBox.Show("Schedule posted.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error posting schedule: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateScheduledHours();
        }


        private void SaveDaySchedule(string dayName, string locationID, SqlConnection conn)
{
    var canvas = GetCanvasContainer(dayName);
    if (canvas == null) return;

    DateTime shiftDate = dayDateMap[dayName];

    foreach (UIElement child in canvas.Children)
    {
        if (child is Rectangle shiftRectangle && shiftToEmployeeMap.TryGetValue(shiftRectangle, out string employeeID))
        {
            double shiftStartHour = Canvas.GetLeft(shiftRectangle) / canvas.ActualWidth * 24;
            double shiftEndHour = (Canvas.GetLeft(shiftRectangle) + shiftRectangle.Width) / canvas.ActualWidth * 24;

            TimeSpan shiftStartTime = TimeSpan.FromHours(shiftStartHour);
            TimeSpan shiftEndTime = TimeSpan.FromHours(shiftEndHour);

            decimal totalBreakTime = 0;
            if (shiftBreaksMap.ContainsKey(shiftRectangle))
            {
                foreach (var breakPair in shiftBreaksMap[shiftRectangle])
                {
                    var breakRectangle = breakPair.Item1;
                    double breakStartHour = Canvas.GetLeft(breakRectangle) / canvas.ActualWidth * 24;
                    double breakEndHour = (Canvas.GetLeft(breakRectangle) + breakRectangle.Width) / canvas.ActualWidth * 24;
                    totalBreakTime += (decimal)(breakEndHour - breakStartHour);
                }
            }

            string insertQuery = @"
                INSERT INTO LocationSchedule 
                (LocationID, EmployeeID, ShiftStartDate, ShiftStartTime, ShiftEndDate, ShiftEndTime, ShiftBreakTotal, ShiftStartDay, ShiftEndDay) 
                VALUES 
                (@LocationID, @EmployeeID, @ShiftStartDate, @ShiftStartTime, @ShiftEndDate, @ShiftEndTime, @ShiftBreakTotal, @ShiftStartDay, @ShiftEndDay)";

            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
            {
                insertCmd.Parameters.AddWithValue("@LocationID", locationID);
                insertCmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                insertCmd.Parameters.AddWithValue("@ShiftStartDate", shiftDate);
                insertCmd.Parameters.AddWithValue("@ShiftStartTime", shiftStartTime);
                insertCmd.Parameters.AddWithValue("@ShiftEndDate", shiftDate);
                insertCmd.Parameters.AddWithValue("@ShiftEndTime", shiftEndTime);
                insertCmd.Parameters.AddWithValue("@ShiftBreakTotal", totalBreakTime);
                insertCmd.Parameters.AddWithValue("@ShiftStartDay", dayName);
                insertCmd.Parameters.AddWithValue("@ShiftEndDay", dayName);

                insertCmd.ExecuteNonQuery();
            }
        }
    }
}

        public class ShiftDialog : Window
        {
            public DateTime StartTime { get;  set; }
            public DateTime EndTime { get;  set; }
            public string SelectedDay { get;  set; }
            public string SelectedEmployeeID { get;  set; }

            public ComboBox _dayPicker;
            public ComboBox _employeePicker;
            public DatePicker _startDatePicker;
            public ComboBox _startTimePicker;
            public ComboBox _endTimePicker;

            public ShiftDialog(Dictionary<string, string> employees, string initialDay)
            {
                Title = "Add Shift";
                Width = 400;
                Height = 300;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

                var grid = new Grid
                {
                    Margin = new Thickness(10)
                };

                // Define rows
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Day
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Employee
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Start Time
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // End Time
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

                // Define columns
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Label
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Input controls

                // Day Picker
                var dayLabel = new TextBlock
                {
                    Text = "Day:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 10, 5)
                };
                Grid.SetRow(dayLabel, 0);
                Grid.SetColumn(dayLabel, 0);
                grid.Children.Add(dayLabel);

                _dayPicker = new ComboBox
                {
                    ItemsSource = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" },
                    SelectedItem = initialDay
                };
                Grid.SetRow(_dayPicker, 0);
                Grid.SetColumn(_dayPicker, 1);
                grid.Children.Add(_dayPicker);

                // Employee Picker
                var employeeLabel = new TextBlock
                {
                    Text = "Employee:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 10, 5)
                };
                Grid.SetRow(employeeLabel, 1);
                Grid.SetColumn(employeeLabel, 0);
                grid.Children.Add(employeeLabel);

                _employeePicker = new ComboBox
                {
                    ItemsSource = employees.Select(e => new KeyValuePair<string, string>(e.Key, e.Value)),
                    DisplayMemberPath = "Value",
                    SelectedValuePath = "Key"
                };
                Grid.SetRow(_employeePicker, 1);
                Grid.SetColumn(_employeePicker, 1);
                grid.Children.Add(_employeePicker);

                // Start Time
                var startTimeLabel = new TextBlock
                {
                    Text = "Start Time:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 10, 5)
                };
                Grid.SetRow(startTimeLabel, 2);
                Grid.SetColumn(startTimeLabel, 0);
                grid.Children.Add(startTimeLabel);

                _startTimePicker = CreateTimePicker();
                _startTimePicker.SelectedItem = StartTime != default ? StartTime.ToString("HH:mm") : "00:00";
                Grid.SetRow(_startTimePicker, 2);
                Grid.SetColumn(_startTimePicker, 1);
                grid.Children.Add(_startTimePicker);

                // End Time
                var endTimeLabel = new TextBlock
                {
                    Text = "End Time:",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 5, 10, 5)
                };
                Grid.SetRow(endTimeLabel, 3);
                Grid.SetColumn(endTimeLabel, 0);
                grid.Children.Add(endTimeLabel);

                _endTimePicker = CreateTimePicker();
                _endTimePicker.SelectedItem = EndTime != default ? EndTime.ToString("HH:mm") : "00:00";
                Grid.SetRow(_endTimePicker, 3);
                Grid.SetColumn(_endTimePicker, 1);
                grid.Children.Add(_endTimePicker);

                // Buttons
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(5) };
                okButton.Click += (s, e) =>
                {
                    SelectedDay = _dayPicker.SelectedItem.ToString();
                    SelectedEmployeeID = ((KeyValuePair<string, string>)_employeePicker.SelectedItem).Key;
                    StartTime = DateTime.Parse((string)_startTimePicker.SelectedItem);
                    EndTime = DateTime.Parse((string)_endTimePicker.SelectedItem);


                    if (EndTime > StartTime)
                    {
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("End time must be after start time.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };

                var cancelButton = new Button { Content = "Cancel", Width = 75, Margin = new Thickness(5) };
                cancelButton.Click += (s, e) => Close();

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                Grid.SetRow(buttonPanel, 4);
                Grid.SetColumnSpan(buttonPanel, 2);
                grid.Children.Add(buttonPanel);

                Content = grid;
            }


            public class BreakDialog : Window
            {
                public DateTime BreakStart { get; private set; }
                public DateTime BreakEnd { get; private set; }

                private DatePicker _datePicker;
                private ComboBox _startTimePicker;
                private ComboBox _endTimePicker;

                public BreakDialog()
                {
                    Title = "Add Break";
                    Width = 350;
                    Height = 250;
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    var grid = new Grid
                    {
                        Margin = new Thickness(10)
                    };

                    // Define rows
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Start Time
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // End Time
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

                    // Start Time Row
                    var startTimeLabel = new TextBlock
                    {
                        Text = "Start Time:",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 5, 10, 5)
                    };
                    Grid.SetRow(startTimeLabel, 0);
                    grid.Children.Add(startTimeLabel);

                    _startTimePicker = CreateTimePicker();
                    Grid.SetRow(_startTimePicker, 0);
                    Grid.SetColumn(_startTimePicker, 1);
                    grid.Children.Add(_startTimePicker);

                    // End Time Row
                    var endTimeLabel = new TextBlock
                    {
                        Text = "End Time:",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 5, 10, 5)
                    };
                    Grid.SetRow(endTimeLabel, 1);
                    grid.Children.Add(endTimeLabel);

                    _endTimePicker = CreateTimePicker();
                    Grid.SetRow(_endTimePicker, 1);
                    Grid.SetColumn(_endTimePicker, 1);
                    grid.Children.Add(_endTimePicker);

                    // Buttons Row
                    var buttonPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };

                    var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(5) };
                    okButton.Click += (s, e) =>
                    {
                        BreakStart = DateTime.Today.Add(TimeSpan.Parse((string)_startTimePicker.SelectedItem));
                        BreakEnd = DateTime.Today.Add(TimeSpan.Parse((string)_endTimePicker.SelectedItem));

                        if (BreakEnd > BreakStart)
                        {
                            DialogResult = true;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("End time must be after start time.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    };

                    var cancelButton = new Button { Content = "Cancel", Width = 75, Margin = new Thickness(5) };
                    cancelButton.Click += (s, e) => Close();

                    buttonPanel.Children.Add(okButton);
                    buttonPanel.Children.Add(cancelButton);

                    Grid.SetRow(buttonPanel, 2);
                    grid.Children.Add(buttonPanel);

                    Content = grid;
                }

                private ComboBox CreateTimePicker()
                {
                    var comboBox = new ComboBox { Margin = new Thickness(5) };

                    // Populate with 12-hour AM/PM times
                    for (int hour = 0; hour < 24; hour++)
                    {
                        for (int quarter = 0; quarter < 4; quarter++)
                        {
                            int totalMinutes = hour * 60 + quarter * 15;
                            DateTime time = DateTime.Today.AddMinutes(totalMinutes);
                            string timeFormatted = time.ToString("hh:mm tt"); // 12-hour format with AM/PM
                            comboBox.Items.Add(timeFormatted);
                        }
                    }

                    comboBox.SelectedIndex = 0; // Default to the first item
                    return comboBox;
                }

            }

            private ComboBox CreateTimePicker()
            {
                var comboBox = new ComboBox { Margin = new Thickness(5) };

                // Populate with 12-hour AM/PM times
                for (int hour = 0; hour < 24; hour++)
                {
                    for (int quarter = 0; quarter < 4; quarter++)
                    {
                        int totalMinutes = hour * 60 + quarter * 15;
                        DateTime time = DateTime.Today.AddMinutes(totalMinutes);
                        string timeFormatted = time.ToString("hh:mm tt"); // 12-hour format with AM/PM
                        comboBox.Items.Add(timeFormatted);
                    }
                }

                comboBox.SelectedIndex = 0; // Default to the first item
                return comboBox;
            }

        }
    }
}
