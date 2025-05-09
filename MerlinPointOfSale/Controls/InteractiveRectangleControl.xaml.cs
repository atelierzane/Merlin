using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static MerlinPointOfSale.Controls.ShiftDialog;

namespace MerlinPointOfSale.Controls
{
    public partial class InteractiveRectangleControl : UserControl
    {
        private Rectangle _activeRectangle;
        private TextBlock _activeTextBlock;
        private Point _initialMousePosition;
        private bool _isDragging;
        private bool _isResizingLeft;
        private bool _isResizingRight;

        private Cursor _openHandCursor;
        private Cursor _closedHandCursor;

        private const double GridIncrement = 35; // Width of each grid column in pixels
        private const double RowHeight = 50; // Height of each row
        private const double ResizeThreshold = 10; // Distance near the edge for resizing
        private const double MinimumWidth = 35; // Minimum width of the rectangle

        private int _rowCount = 0;
        private TimeSpan _totalScheduledTime = TimeSpan.Zero;
        private readonly Dictionary<Rectangle, TextBlock> _rectangleTextBlockMap = new();
        private readonly Dictionary<Rectangle, List<Rectangle>> _shiftBreakMap = new();
        private readonly Dictionary<Rectangle, Rectangle> _breakParentMap = new();



        public InteractiveRectangleControl()
        {
            InitializeComponent();
            InitializeCursors();
            InitializeTimeGrid();

            CanvasContainer.MouseMove += OnMouseMove;
            CanvasContainer.MouseLeftButtonUp += OnMouseLeftButtonUp;

        }

        private void InitializeTimeGrid()
        {
            TimeGrid.ColumnDefinitions.Clear();
            TimeGrid.Children.Clear();
            EmployeeGrid.RowDefinitions.Clear();
            GridLines.Children.Clear();

            double canvasHeight = CanvasContainer.ActualHeight > 0 ? CanvasContainer.ActualHeight : 600;
            double totalWidth = 24 * 4 * GridIncrement; // 24 hours * 4 increments per hour

            CanvasContainer.Width = totalWidth; // Set the total width of the canvas for scrolling
            GridLines.Width = totalWidth;

            for (int hour = 0; hour < 24; hour++)
            {
                for (int quarter = 0; quarter < 4; quarter++)
                {
                    TimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridIncrement) });

                    if (quarter == 0)
                    {
                        var timeLabel = new TextBlock
                        {
                            Text = $"{hour:D2}:00",
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 5, 0, 0),
                            FontWeight = FontWeights.Bold
                        };
                        Grid.SetColumn(timeLabel, TimeGrid.ColumnDefinitions.Count - 1);
                        TimeGrid.Children.Add(timeLabel);
                    }

                    var verticalLine = new Line
                    {
                        X1 = TimeGrid.ColumnDefinitions.Count * GridIncrement - (GridIncrement / 2),
                        Y1 = 0,
                        X2 = TimeGrid.ColumnDefinitions.Count * GridIncrement - (GridIncrement / 2),
                        Y2 = canvasHeight,
                        Stroke = quarter == 0 ? Brushes.DarkGray : Brushes.LightGray, // Whole hours are darker
                        StrokeThickness = quarter == 0 ? 1.5 : 0.5 // Whole hours are thicker
                    };
                    GridLines.Children.Add(verticalLine);
                }
            }

            Grid.SetColumnSpan(CanvasContainer, TimeGrid.ColumnDefinitions.Count);
            Grid.SetColumnSpan(GridLines, TimeGrid.ColumnDefinitions.Count);
        }

        private void InitializeCursors()
        {
            // Use Windows default cursors
            _openHandCursor = Cursors.Hand;       // Open hand for hover
            _closedHandCursor = Cursors.SizeAll; // Closed hand for dragging
        }

        private Cursor LoadCursorFromResource(string resourcePath)
        {
            using (Stream cursorStream = Application.GetResourceStream(new Uri(resourcePath, UriKind.Relative)).Stream)
            {
                return new Cursor(cursorStream);
            }
        }

        private void AddEmployeeLabel(string employeeName)
        {
            // Add a row definition for the new employee
            EmployeeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(RowHeight) });

            // Create a label for the employee name
            var employeeLabel = new TextBlock
            {
                Text = employeeName,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetRow(employeeLabel, _rowCount);
            EmployeeGrid.Children.Add(employeeLabel);
        }
        private void AddRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            var shiftDialog = new ShiftDialog();
            if (shiftDialog.ShowDialog() == true)
            {
                DateTime startTime = shiftDialog.StartTime;
                DateTime endTime = shiftDialog.EndTime;

                if (endTime <= startTime)
                {
                    MessageBox.Show("End time must be later than start time.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string employeeName = $"Employee {_rowCount + 1}";
                AddEmployeeLabel(employeeName);

                double left = CalculatePosition(startTime);
                double width = CalculatePosition(endTime) - left;

                CreateShiftRectangle(left, _rowCount * RowHeight, width, startTime, endTime);

                _rowCount++;

                UpdateTotalScheduledTime(); // Update after adding shift
            }
        }

        private void CreateShiftRectangle(double left, double top, double width, DateTime startTime, DateTime endTime)
        {
            var rectangle = new Rectangle
            {
                Width = width,
                Height = RowHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.LightBlue,
                Tag = new Tuple<DateTime, DateTime>(startTime, endTime)
            };

            var textBlock = new TextBlock
            {
                Text = FormatShiftDetails(startTime, endTime),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            Canvas.SetLeft(textBlock, left + 5);
            Canvas.SetTop(textBlock, top + 5);

            CanvasContainer.Children.Add(rectangle);
            CanvasContainer.Children.Add(textBlock);

            _rectangleTextBlockMap[rectangle] = textBlock;
            _shiftBreakMap[rectangle] = new List<Rectangle>(); // Initialize break list

            rectangle.ContextMenu = CreateContextMenu(rectangle);

            rectangle.MouseLeftButtonDown += (sender, e) =>
            {
                _activeRectangle = rectangle;
                _activeTextBlock = textBlock;
                _initialMousePosition = e.GetPosition(CanvasContainer);

                double mouseX = _initialMousePosition.X;
                double rectLeft = Canvas.GetLeft(rectangle);
                double rectRight = rectLeft + rectangle.Width;

                _isDragging = !(Math.Abs(mouseX - rectLeft) <= ResizeThreshold || Math.Abs(mouseX - rectRight) <= ResizeThreshold);
                _isResizingLeft = Math.Abs(mouseX - rectLeft) <= ResizeThreshold;
                _isResizingRight = Math.Abs(mouseX - rectRight) <= ResizeThreshold;

                rectangle.CaptureMouse();
                e.Handled = true;
            };
        }






        private double CalculatePosition(DateTime time)
        {
            int totalMinutes = time.Hour * 60 + time.Minute;
            return (totalMinutes / 15.0) * GridIncrement + (GridIncrement / 2); // Align to the right center of grid lines
        }


        private DateTime CalculateTime(double position)
        {
            position -= (GridIncrement / 2); // Reverse the alignment offset
            int totalMinutes = (int)(position / GridIncrement * 15);
            return DateTime.Today.AddMinutes(totalMinutes);
        }

        private string FormatShiftDetails(DateTime startTime, DateTime endTime)
        {
            var duration = endTime - startTime;
            return $"{startTime:HH:mm} - {endTime:HH:mm}\n({duration.Hours}h {duration.Minutes}m)";
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_activeRectangle == null || e.LeftButton != MouseButtonState.Pressed)
            {
                // Update cursor when hovering over rectangles
                UpdateCursor(e.GetPosition(CanvasContainer));
                return;
            }

            Point currentMousePosition = e.GetPosition(CanvasContainer);
            double offsetX = currentMousePosition.X - _initialMousePosition.X;

            // Check if the active rectangle is a break
            if (_breakParentMap.TryGetValue(_activeRectangle, out var parentShiftRectangle))
            {
                // Handle movement or resizing of a break
                if (_isResizingLeft)
                {
                    double newLeft = Canvas.GetLeft(_activeRectangle) + offsetX;
                    double newWidth = _activeRectangle.Width - offsetX;

                    double parentLeft = Canvas.GetLeft(parentShiftRectangle);
                    if (newLeft >= parentLeft && newWidth >= MinimumWidth)
                    {
                        Canvas.SetLeft(_activeRectangle, newLeft);
                        _activeRectangle.Width = newWidth;
                        UpdateBreakDetails(_activeRectangle, parentShiftRectangle);
                        _initialMousePosition = currentMousePosition;
                    }
                }
                else if (_isResizingRight)
                {
                    double newWidth = _activeRectangle.Width + offsetX;

                    double parentRight = Canvas.GetLeft(parentShiftRectangle) + parentShiftRectangle.Width;
                    if (Canvas.GetLeft(_activeRectangle) + newWidth <= parentRight && newWidth >= MinimumWidth)
                    {
                        _activeRectangle.Width = newWidth;
                        UpdateBreakDetails(_activeRectangle, parentShiftRectangle);
                        _initialMousePosition = currentMousePosition;
                    }
                }
                else if (_isDragging)
                {
                    double newLeft = Canvas.GetLeft(_activeRectangle) + offsetX;

                    double parentLeft = Canvas.GetLeft(parentShiftRectangle);
                    double parentRight = parentLeft + parentShiftRectangle.Width;
                    if (newLeft >= parentLeft && newLeft + _activeRectangle.Width <= parentRight)
                    {
                        Canvas.SetLeft(_activeRectangle, newLeft);
                        UpdateBreakDetails(_activeRectangle, parentShiftRectangle);
                        _initialMousePosition = currentMousePosition;
                    }
                }
            }
            else
            {
                // Handle movement or resizing of a shift
                if (_isResizingLeft)
                {
                    double newLeft = Canvas.GetLeft(_activeRectangle) + offsetX;
                    double newWidth = _activeRectangle.Width - offsetX;

                    if (newWidth >= MinimumWidth && newLeft >= 0)
                    {
                        Canvas.SetLeft(_activeRectangle, newLeft);
                        _activeRectangle.Width = newWidth;
                        UpdateShiftDetails();
                        _initialMousePosition = currentMousePosition;
                    }
                }
                else if (_isResizingRight)
                {
                    double newWidth = _activeRectangle.Width + offsetX;

                    if (newWidth >= MinimumWidth)
                    {
                        _activeRectangle.Width = newWidth;
                        UpdateShiftDetails();
                        _initialMousePosition = currentMousePosition;
                    }
                }
                else if (_isDragging)
                {
                    double newLeft = Canvas.GetLeft(_activeRectangle) + offsetX;

                    if (newLeft >= 0 && newLeft + _activeRectangle.Width <= CanvasContainer.ActualWidth)
                    {
                        Canvas.SetLeft(_activeRectangle, newLeft);
                        Canvas.SetLeft(_activeTextBlock, newLeft + 5); // Move text with rectangle
                        UpdateShiftDetails();
                        _initialMousePosition = currentMousePosition;
                    }
                }
            }
        }


        private void OnMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (_activeRectangle != null)
            {
                _activeRectangle.ReleaseMouseCapture();
                _activeRectangle = null;
                _activeTextBlock = null;
                _isDragging = false;
                _isResizingLeft = false;
                _isResizingRight = false;
                Cursor = Cursors.Arrow; // Reset to default cursor
            }
        }

        private void UpdateShiftDetails()
        {
            if (_activeRectangle == null || _activeTextBlock == null)
                return;

            double left = Canvas.GetLeft(_activeRectangle);
            double right = left + _activeRectangle.Width;

            DateTime startTime = CalculateTime(left);
            DateTime endTime = CalculateTime(right);

            _activeTextBlock.Text = FormatShiftDetails(startTime, endTime);
            Canvas.SetLeft(_activeTextBlock, left + 5);

            _activeRectangle.Tag = new Tuple<DateTime, DateTime>(startTime, endTime);

            // Move associated breaks and update their start and end times
            if (_shiftBreakMap.TryGetValue(_activeRectangle, out var breaks))
            {
                foreach (var breakRectangle in breaks)
                {
                    if (breakRectangle.Tag is Tuple<DateTime, DateTime, double> breakInfo)
                    {
                        var breakOffset = breakInfo.Item3; // Offset relative to the shift rectangle
                        double breakLeft = left + breakOffset;

                        // Recalculate break start and end times based on the new shift position
                        DateTime breakStart = CalculateTime(breakLeft);
                        DateTime breakEnd = CalculateTime(breakLeft + breakRectangle.Width);

                        // Update the position and width of the break rectangle
                        Canvas.SetLeft(breakRectangle, breakLeft);
                        breakRectangle.Width = CalculatePosition(breakEnd) - CalculatePosition(breakStart);

                        // Update the break text
                        if (_rectangleTextBlockMap.TryGetValue(breakRectangle, out var breakLabel))
                        {
                            breakLabel.Text = FormatBreakDetails(breakStart, breakEnd);
                            Canvas.SetLeft(breakLabel, breakLeft + 5);
                        }

                        // Update the break's tag to reflect the new times
                        breakRectangle.Tag = new Tuple<DateTime, DateTime, double>(breakStart, breakEnd, breakOffset);
                    }
                }
            }

            UpdateTotalScheduledTime();
        }




        private void UpdateTotalScheduledTime()
        {
            _totalScheduledTime = TimeSpan.Zero;

            foreach (var shift in _shiftBreakMap)
            {
                var shiftRectangle = shift.Key;
                var breaks = shift.Value;

                if (shiftRectangle.Tag is Tuple<DateTime, DateTime> shiftTimes)
                {
                    var shiftStart = shiftTimes.Item1;
                    var shiftEnd = shiftTimes.Item2;
                    var shiftDuration = shiftEnd - shiftStart;

                    // Deduct break durations from the shift duration
                    foreach (var breakRectangle in breaks)
                    {
                        if (breakRectangle.Tag is Tuple<DateTime, DateTime, double> breakInfo)
                        {
                            var breakStart = breakInfo.Item1;
                            var breakEnd = breakInfo.Item2;

                            // Ensure the break is within the shift bounds
                            if (breakStart >= shiftStart && breakEnd <= shiftEnd)
                            {
                                shiftDuration -= (breakEnd - breakStart);
                            }
                        }
                    }

                    _totalScheduledTime += shiftDuration;
                }
            }

            // Update the UI to display total scheduled hours
            TotalHoursTextBlock.Text = $"Total Scheduled Hours: {_totalScheduledTime.Hours}h {_totalScheduledTime.Minutes}m";
        }



        private void UpdateCursor(Point mousePosition)
        {
            bool isCursorSet = false; // Flag to check if a cursor has already been set

            foreach (var child in CanvasContainer.Children)
            {
                if (child is Rectangle rectangle)
                {
                    double rectLeft = Canvas.GetLeft(rectangle);
                    double rectRight = rectLeft + rectangle.Width;
                    double rectTop = Canvas.GetTop(rectangle);
                    double rectBottom = rectTop + rectangle.Height;

                    // Check if the mouse is near the edges for resizing
                    if (mousePosition.Y >= rectTop && mousePosition.Y <= rectBottom)
                    {
                        if (mousePosition.X >= rectLeft - ResizeThreshold && mousePosition.X <= rectLeft + ResizeThreshold)
                        {
                            Cursor = Cursors.SizeWE; // Left edge - resize
                            isCursorSet = true;
                            break;
                        }
                        else if (mousePosition.X >= rectRight - ResizeThreshold && mousePosition.X <= rectRight + ResizeThreshold)
                        {
                            Cursor = Cursors.SizeWE; // Right edge - resize
                            isCursorSet = true;
                            break;
                        }
                    }

                    // Check if the mouse is over the rectangle for dragging
                    if (mousePosition.X > rectLeft + ResizeThreshold && mousePosition.X < rectRight - ResizeThreshold &&
                        mousePosition.Y > rectTop && mousePosition.Y < rectBottom)
                    {
                        Cursor = _openHandCursor; // Hand cursor for dragging
                        isCursorSet = true;
                        break;
                    }
                }
            }

            // Reset to default cursor if no rectangle is hovered
            if (!isCursorSet)
            {
                Cursor = Cursors.Arrow;
            }
        }



        private ContextMenu CreateContextMenu(Rectangle rectangle)
        {
            var contextMenu = new ContextMenu();

            var removeMenuItem = new MenuItem
            {
                Header = "Remove Shift"
            };
            removeMenuItem.Click += (sender, e) => RemoveShift(rectangle);

            var addBreakMenuItem = new MenuItem
            {
                Header = "Add Break"
            };
            addBreakMenuItem.Click += (sender, e) => AddBreakToShift(rectangle);

            var removeBreakMenuItem = new MenuItem
            {
                Header = "Remove Break",
                IsEnabled = _shiftBreakMap.ContainsKey(rectangle) && _shiftBreakMap[rectangle].Count > 0
            };
            removeBreakMenuItem.Click += (sender, e) => RemoveBreakFromShift(rectangle);

            var changeColorMenuItem = new MenuItem
            {
                Header = "Change Color"
            };
            changeColorMenuItem.Click += (sender, e) => ShowColorPicker(rectangle);

            contextMenu.Items.Add(removeMenuItem);
            contextMenu.Items.Add(addBreakMenuItem);
            contextMenu.Items.Add(removeBreakMenuItem);
            contextMenu.Items.Add(changeColorMenuItem);

            return contextMenu;
        }


        private void ShowColorPicker(Rectangle rectangle)
        {
            var colorPickerWindow = new Window
            {
                Title = "Select Color",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            var predefinedColors = new Dictionary<string, SolidColorBrush>
    {
        { "Light Blue", Brushes.LightBlue },
        { "Orange", Brushes.Orange },
        { "Green", Brushes.Green },
        { "Yellow", Brushes.Yellow },
        { "Pink", Brushes.Pink },
        { "Gray", Brushes.Gray },
        { "Red", Brushes.Red }
    };

            var colorComboBox = new ComboBox
            {
                ItemsSource = predefinedColors,
                DisplayMemberPath = "Key",
                SelectedValuePath = "Value",
                Margin = new Thickness(0, 0, 0, 10)
            };

            var applyButton = new Button
            {
                Content = "Apply",
                Width = 75,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            applyButton.Click += (sender, e) =>
            {
                if (colorComboBox.SelectedValue is SolidColorBrush selectedBrush)
                {
                    rectangle.Fill = selectedBrush;

                    // Optionally, apply color to associated breaks
                    if (_shiftBreakMap.TryGetValue(rectangle, out var breaks))
                    {
                        foreach (var breakRectangle in breaks)
                        {
                            breakRectangle.Fill = selectedBrush;
                        }
                    }

                    colorPickerWindow.Close();
                }
            };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Select a Color:",
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.Bold
            });

            stackPanel.Children.Add(colorComboBox);
            stackPanel.Children.Add(applyButton);

            colorPickerWindow.Content = stackPanel;
            colorPickerWindow.ShowDialog();
        }


        private void RemoveShift(Rectangle shiftRectangle)
        {
            // Remove associated breaks
            if (_shiftBreakMap.TryGetValue(shiftRectangle, out var breaks))
            {
                foreach (var breakRectangle in breaks.ToList())
                {
                    // Remove the break rectangle and its associated text block
                    if (_rectangleTextBlockMap.TryGetValue(breakRectangle, out var textBlock))
                    {
                        CanvasContainer.Children.Remove(textBlock);
                        _rectangleTextBlockMap.Remove(breakRectangle);
                    }

                    CanvasContainer.Children.Remove(breakRectangle);
                    _breakParentMap.Remove(breakRectangle);
                }

                breaks.Clear(); // Clear the breaks list for the shift
            }

            // Remove the shift's text block
            if (_rectangleTextBlockMap.TryGetValue(shiftRectangle, out var shiftTextBlock))
            {
                CanvasContainer.Children.Remove(shiftTextBlock);
                _rectangleTextBlockMap.Remove(shiftRectangle);
            }

            // Remove the shift rectangle itself
            CanvasContainer.Children.Remove(shiftRectangle);

            // Remove the shift from the break map
            _shiftBreakMap.Remove(shiftRectangle);

            // Update total scheduled time
            UpdateTotalScheduledTime();
        }


        private void AddBreakToShift(Rectangle shiftRectangle)
        {
            if (shiftRectangle.Tag is Tuple<DateTime, DateTime> times)
            {
                var breakDialog = new BreakDialog();
                if (breakDialog.ShowDialog() == true)
                {
                    var breakStart = breakDialog.BreakStart;
                    var breakEnd = breakDialog.BreakEnd;

                    if (breakStart < times.Item1 || breakEnd > times.Item2)
                    {
                        MessageBox.Show("Break must be within the shift duration.", "Invalid Break", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    double shiftLeft = Canvas.GetLeft(shiftRectangle);
                    double breakLeft = CalculatePosition(breakStart);
                    double breakWidth = CalculatePosition(breakEnd) - breakLeft;
                    double breakOffset = breakLeft - shiftLeft; // Offset relative to the shift rectangle
                    double top = Canvas.GetTop(shiftRectangle) + RowHeight / 4;

                    var breakRectangle = new Rectangle
                    {
                        Width = breakWidth,
                        Height = RowHeight / 2,
                        Fill = Brushes.Orange,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Tag = new Tuple<DateTime, DateTime, double>(breakStart, breakEnd, breakOffset) // Include offset
                    };

                    var breakTextBlock = new TextBlock
                    {
                        Text = FormatBreakDetails(breakStart, breakEnd),
                        FontSize = 10,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        TextAlignment = TextAlignment.Center
                    };

                    Canvas.SetLeft(breakRectangle, breakLeft);
                    Canvas.SetTop(breakRectangle, top);
                    Canvas.SetLeft(breakTextBlock, breakLeft + 5);
                    Canvas.SetTop(breakTextBlock, top + 5);

                    CanvasContainer.Children.Add(breakRectangle);
                    CanvasContainer.Children.Add(breakTextBlock);

                    _rectangleTextBlockMap[breakRectangle] = breakTextBlock;

                    // Link the break to its shift
                    _shiftBreakMap[shiftRectangle].Add(breakRectangle);
                    _breakParentMap[breakRectangle] = shiftRectangle;

                    AddBreakHandlers(breakRectangle, shiftRectangle);

                    // Update the context menu state
                    shiftRectangle.ContextMenu = CreateContextMenu(shiftRectangle);

                    UpdateTotalScheduledTime();
                }
            }
        }


        private void AddBreakHandlers(Rectangle breakRectangle, Rectangle parentShiftRectangle)
        {
            breakRectangle.MouseLeftButtonDown += (sender, e) =>
            {
                _activeRectangle = breakRectangle;
                _initialMousePosition = e.GetPosition(CanvasContainer);

                double mouseX = _initialMousePosition.X;
                double rectLeft = Canvas.GetLeft(breakRectangle);
                double rectRight = rectLeft + breakRectangle.Width;

                _isDragging = !(Math.Abs(mouseX - rectLeft) <= ResizeThreshold || Math.Abs(mouseX - rectRight) <= ResizeThreshold);
                _isResizingLeft = Math.Abs(mouseX - rectLeft) <= ResizeThreshold;
                _isResizingRight = Math.Abs(mouseX - rectRight) <= ResizeThreshold;

                breakRectangle.CaptureMouse();
                e.Handled = true;
            };

            breakRectangle.MouseMove += (sender, e) =>
            {
                if (_activeRectangle == null || e.LeftButton != MouseButtonState.Pressed) return;

                Point currentMousePosition = e.GetPosition(CanvasContainer);
                double offsetX = currentMousePosition.X - _initialMousePosition.X;

                double parentLeft = Canvas.GetLeft(parentShiftRectangle);
                double parentRight = parentLeft + parentShiftRectangle.Width;

                if (_isResizingLeft)
                {
                    double newLeft = Canvas.GetLeft(breakRectangle) + offsetX;
                    double newWidth = breakRectangle.Width - offsetX;

                    if (newLeft >= parentLeft && newWidth >= MinimumWidth)
                    {
                        Canvas.SetLeft(breakRectangle, newLeft);
                        breakRectangle.Width = newWidth;
                        UpdateBreakDetails(breakRectangle, parentShiftRectangle);
                        _initialMousePosition = currentMousePosition;
                    }
                }
                else if (_isResizingRight)
                {
                    double newWidth = breakRectangle.Width + offsetX;

                    if (Canvas.GetLeft(breakRectangle) + newWidth <= parentRight && newWidth >= MinimumWidth)
                    {
                        breakRectangle.Width = newWidth;
                        UpdateBreakDetails(breakRectangle, parentShiftRectangle);
                        _initialMousePosition = currentMousePosition;
                    }
                }
                else if (_isDragging)
                {
                    double newLeft = Canvas.GetLeft(breakRectangle) + offsetX;

                    if (newLeft >= parentLeft && newLeft + breakRectangle.Width <= parentRight)
                    {
                        Canvas.SetLeft(breakRectangle, newLeft);
                        UpdateBreakDetails(breakRectangle, parentShiftRectangle);
                        _initialMousePosition = currentMousePosition;
                    }
                }
            };

            breakRectangle.MouseLeftButtonUp += (sender, e) =>
            {
                if (_activeRectangle != null)
                {
                    _activeRectangle.ReleaseMouseCapture();
                    _activeRectangle = null;
                    _isDragging = false;
                    _isResizingLeft = false;
                    _isResizingRight = false;
                    Cursor = Cursors.Arrow; // Reset to default cursor
                }
            };
        }





        private void UpdateBreakDetails(Rectangle breakRectangle, Rectangle parentShiftRectangle)
        {
            if (parentShiftRectangle == null)
            {
                MessageBox.Show("Parent shift rectangle is not associated with the break.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            double breakLeft = Canvas.GetLeft(breakRectangle);
            double breakRight = breakLeft + breakRectangle.Width;

            DateTime breakStart = CalculateTime(breakLeft);
            DateTime breakEnd = CalculateTime(breakRight);

            if (_rectangleTextBlockMap.TryGetValue(breakRectangle, out var textBlock))
            {
                textBlock.Text = FormatBreakDetails(breakStart, breakEnd);
                Canvas.SetLeft(textBlock, breakLeft + 5);
            }

            // Update the break's tag with the new details
            if (breakRectangle.Tag is Tuple<DateTime, DateTime, double> breakInfo)
            {
                double parentLeft = Canvas.GetLeft(parentShiftRectangle);
                double breakOffset = breakLeft - parentLeft; // New offset relative to the parent shift
                breakRectangle.Tag = new Tuple<DateTime, DateTime, double>(breakStart, breakEnd, breakOffset);
            }

            UpdateTotalScheduledTime();
        }


        private void RemoveBreakFromShift(Rectangle shiftRectangle)
        {
            if (_shiftBreakMap.TryGetValue(shiftRectangle, out var breaks) && breaks.Count > 0)
            {
                // Select the break to remove (or remove all if needed)
                var breakToRemove = breaks.LastOrDefault(); // Example: remove the last break
                if (breakToRemove != null)
                {
                    // Remove the break rectangle and its associated text block
                    if (_rectangleTextBlockMap.TryGetValue(breakToRemove, out var textBlock))
                    {
                        CanvasContainer.Children.Remove(textBlock);
                        _rectangleTextBlockMap.Remove(breakToRemove);
                    }

                    CanvasContainer.Children.Remove(breakToRemove);
                    _breakParentMap.Remove(breakToRemove);

                    // Remove the break from the shift's break list
                    breaks.Remove(breakToRemove);
                }

                // Update the context menu state
                shiftRectangle.ContextMenu = CreateContextMenu(shiftRectangle);

                // Update total scheduled time
                UpdateTotalScheduledTime();
            }
        }




        private string FormatBreakDetails(DateTime breakStart, DateTime breakEnd)
        {
            var duration = breakEnd - breakStart;
            return $"{breakStart:HH:mm} - {breakEnd:HH:mm}\n({duration.Hours}h {duration.Minutes}m)";
        }






    }

    public class ShiftDialog : Window
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    private DatePicker _startDatePicker;
    private DatePicker _endDatePicker;
    private ComboBox _startTimePicker;
    private ComboBox _endTimePicker;

    public ShiftDialog()
    {
        Title = "Add Shift";
        Width = 350;
        Height = 250;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var grid = new Grid
        {
            Margin = new Thickness(10)
        };

        // Define rows
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Start Time
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // End Time
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

        // Define columns
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Label
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Input controls

        // Start Time Row
        var startTimeLabel = new TextBlock
        {
            Text = "Start Time:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 10, 5)
        };
        Grid.SetRow(startTimeLabel, 1);
        Grid.SetColumn(startTimeLabel, 0);
        grid.Children.Add(startTimeLabel);

        _startDatePicker = new DatePicker { SelectedDate = DateTime.Today };
        Grid.SetRow(_startDatePicker, 1);
        Grid.SetColumn(_startDatePicker, 1);
        grid.Children.Add(_startDatePicker);

        _startTimePicker = CreateTimePicker();
        Grid.SetRow(_startTimePicker, 1);
        Grid.SetColumn(_startTimePicker, 2);
        grid.Children.Add(_startTimePicker);

        // End Time Row
        var endTimeLabel = new TextBlock
        {
            Text = "End Time:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 10, 5)
        };
        Grid.SetRow(endTimeLabel, 2);
        Grid.SetColumn(endTimeLabel, 0);
        grid.Children.Add(endTimeLabel);

        _endDatePicker = new DatePicker { SelectedDate = DateTime.Today };
        Grid.SetRow(_endDatePicker, 2);
        Grid.SetColumn(_endDatePicker, 1);
        grid.Children.Add(_endDatePicker);

        _endTimePicker = CreateTimePicker();
        Grid.SetRow(_endTimePicker, 2);
        Grid.SetColumn(_endTimePicker, 2);
        grid.Children.Add(_endTimePicker);

        // Buttons Row
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(5) };
        var cancelButton = new Button { Content = "Cancel", Width = 75, Margin = new Thickness(5) };

        okButton.Click += (s, e) =>
        {
            StartTime = _startDatePicker.SelectedDate.Value.Add(TimeSpan.Parse((string)_startTimePicker.SelectedItem));
            EndTime = _endDatePicker.SelectedDate.Value.Add(TimeSpan.Parse((string)_endTimePicker.SelectedItem));
            DialogResult = true;
            Close();
        };

        cancelButton.Click += (s, e) => Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        Grid.SetRow(buttonPanel, 3);
        Grid.SetColumnSpan(buttonPanel, 3);
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
                for (int hour = 0; hour < 24; hour++)
                {
                    for (int quarter = 0; quarter < 4; quarter++)
                    {
                        string time = $"{hour:D2}:{quarter * 15:D2}";
                        comboBox.Items.Add(time);
                    }
                }
                comboBox.SelectedIndex = 0;
                return comboBox;
            }
        }


        private ComboBox CreateTimePicker()
    {
        var comboBox = new ComboBox { Margin = new Thickness(5) };
        for (int hour = 0; hour < 24; hour++)
        {
            for (int quarter = 0; quarter < 4; quarter++)
            {
                string time = $"{hour:D2}:{quarter * 15:D2}";
                comboBox.Items.Add(time);
            }
        }
        comboBox.SelectedIndex = 0;
        return comboBox;
    }
}

}

