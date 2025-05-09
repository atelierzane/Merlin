using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using MerlinPointOfSale.Windows.AlertWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Windows.Data;


namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for TransactionLoginWindow.xaml
    /// </summary>
    public partial class TransactionLoginWindow : Window
    {
        private DatabaseHelper databaseHelper = new DatabaseHelper();

        private VisualHost visualHost = new VisualHost();
        private DrawingVisual glowEffectVisual = new DrawingVisual();
        private Brush originalBrush;

        public string EmployeeInitials { get; set; }
        public string TransactionPIN { get; set; }
        public string EmployeeID { get; private set; }
        public string EmployeeFirstName { get; private set; }

        public string EnteredEmployeeInitials { get; private set; }

        public string EnteredTransactionPIN { get; private set; }
        public TransactionLoginWindow()
        {
            InitializeComponent();

        }

        private void OnButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply blur effect
            var blurEffect = new WindowBlurEffect(this) { BlurOpacity = 1.0f };
            blurEffect.EnableBlur();

            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top + 50,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(Window.TopProperty, topAnimation);

            // Start window animation
            var storyboard = this.FindResource("WindowAnimation_Legacy") as Storyboard;
            storyboard?.Begin();


            if (mainBorder.BorderBrush is LinearGradientBrush brush)
            {
                originalBrush = brush.Clone();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check if left mouse button is pressed
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Move the window
                this.DragMove();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);

            AdjustBorderGlow(mousePosition); // This works as expected.
            UpdateSeparatorGlow(mousePosition); // Ensure this directly updates the separator's glow based on `mousePosition`.
            UpdateSeparatorGlowBG(mousePosition); // Similarly, update the background's glow directly.
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            FadeOutGlowEffect(); // Existing logic that fades out the border's glow effect.

            // Reset the separator's and separator background's glow effect.
            ResetSeparatorGlow();
            ResetSeparatorBackgroundGlow();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // Fade in to full opacity
            AnimateWindowOpacity(0.45, 1.0);
            // Animate removing the blur effect
            AnimateBlurEffect(2.5, 0); // Adjust the starting radius for the blur to remove
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Fade out to lower opacity
            AnimateWindowOpacity(1.0, 0.45);
            // Animate applying a blur effect
            AnimateBlurEffect(0, 2.5); // Adjust the target radius for the blur to apply
        }

        private void AdjustBorderGlow(Point mousePosition)
        {
            double width = mainBorder.ActualWidth;
            double height = mainBorder.ActualHeight;

            // Calculate the relative position of the mouse within the border
            double offsetX = mousePosition.X / width;
            double offsetY = mousePosition.Y / height;

            // Retrieve colors from the resource dictionary
            Color originalColor = ((Color)Application.Current.Resources["windowBorderColor_merlinCyan_GradientStop1"]);
            Color lighterColor = ((Color)Application.Current.Resources["merlinCyanBorder_Highlight"]);

            RadialGradientBrush brush = new RadialGradientBrush
            {
                GradientOrigin = new Point(offsetX, offsetY),
                Center = new Point(offsetX, offsetY),
                RadiusX = 0.225, // Adjust for broader or more localized effect
                RadiusY = 0.225
            };

            // Use the lighter color for the inner gradient stop
            brush.GradientStops.Add(new GradientStop(lighterColor, 0));

            // Transition smoothly to the original color
            brush.GradientStops.Add(new GradientStop(originalColor, 1));

            mainBorder.BorderBrush = brush;
        }

        private void UpdateSeparatorGlow(Point mousePosition)
        {
            // Calculate the Y distance to determine glow intensity.
            double separatorYRelativeToWindow = glowSeparator.TransformToAncestor(this).Transform(new Point(0, 0)).Y + glowSeparator.ActualHeight / 2;
            double distanceYToSeparator = Math.Abs(mousePosition.Y - separatorYRelativeToWindow);
            double glowIntensity = Math.Max(0, 1 - (distanceYToSeparator / 100)); // Adjust this calculation as needed.

            // Calculate the X position for the glow effect to "follow" the mouse.
            double relativeXPosition = mousePosition.X / this.ActualWidth;

            ApplyGlowEffectToSeparator(glowIntensity, relativeXPosition);
        }

        private void UpdateSeparatorGlowBG(Point mousePosition)
        {
            // Calculate the Y distance to determine glow intensity.
            double separatorYRelativeToWindow = glowSeparatorBG.TransformToAncestor(this).Transform(new Point(0, 0)).Y + glowSeparatorBG.ActualHeight / 2;
            double distanceYToSeparator = Math.Abs(mousePosition.Y - separatorYRelativeToWindow);
            double glowIntensity = Math.Max(0, 1 - (distanceYToSeparator / 100)); // Adjust this calculation as needed.

            // Calculate the X position for the glow effect to "follow" the mouse.
            double relativeXPosition = mousePosition.X / this.ActualWidth;

            ApplyGlowEffectToSeparatorBG(glowIntensity, relativeXPosition);
        }

        private void ApplyGlowEffectToSeparator(double intensity, double relativeXPosition)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            // Calculate offsets for glow effect
            double startOffset = Math.Max(0, relativeXPosition - 0.1 * intensity);
            double endOffset = Math.Min(1, relativeXPosition + 0.1 * intensity);

            // Retrieve color from resource dictionary
            Color glowColor = (Color)Application.Current.Resources["merlinCyan"];

            brush.GradientStops.Add(new GradientStop(Colors.Transparent, startOffset));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * intensity), glowColor.R, glowColor.G, glowColor.B), relativeXPosition));
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, endOffset));

            glowSeparator.Fill = brush;
        }

        private void ApplyGlowEffectToSeparatorBG(double intensity, double relativeXPosition)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            // Calculate offsets for glow effect
            double startOffset = Math.Max(0, relativeXPosition - 0.15 * intensity);
            double endOffset = Math.Min(1, relativeXPosition + 0.15 * intensity);

            // Retrieve color from resource dictionary
            Color glowColor = (Color)Application.Current.Resources["merlinCyan"];

            brush.GradientStops.Add(new GradientStop(Colors.Transparent, startOffset));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * intensity), glowColor.R, glowColor.G, glowColor.B), relativeXPosition));
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, endOffset));

            glowSeparatorBG.Fill = brush;
        }

        private void ResetSeparatorGlow()
        {
            // Assuming the default or no glow state for the separator
            glowSeparator.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void ResetSeparatorBackgroundGlow()
        {
            // Assuming the default or no glow state for the separator background
            glowSeparatorBG.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void FadeOutGlowEffect()
        {
            var brush = mainBorder.BorderBrush as RadialGradientBrush;
            if (brush != null)
            {
                // Assuming there are two gradient stops in the brush that need to be animated back to original colors
                Color originalStartColor = Color.FromRgb(27, 73, 167); // Define original start color
                Color originalEndColor = Color.FromRgb(27, 73, 167); // Define original end color, assuming it's the same for simplicity

                // Create and configure the color animations for start and end gradient stops
                ColorAnimation startColorAnimation = new ColorAnimation
                {
                    To = originalStartColor,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                ColorAnimation endColorAnimation = new ColorAnimation
                {
                    To = originalEndColor,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                // Apply the animations to the respective gradient stops
                if (brush.GradientStops.Count > 0)
                    brush.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, startColorAnimation);

                if (brush.GradientStops.Count > 1)
                    brush.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, endColorAnimation);
            }

        }

        private void AnimateBlurEffect(double fromRadius, double toRadius)
        {
            BlurEffect blurEffect = new BlurEffect();
            this.Effect = blurEffect;

            DoubleAnimation blurAnimation = new DoubleAnimation()
            {
                From = fromRadius,
                To = toRadius,
                Duration = TimeSpan.FromSeconds(0.3), // Adjust the duration as needed
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } // Optional easing function
            };

            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
        }

        private void AnimateWindowOpacity(double from, double to)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.3), // Adjust duration as needed
                EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
            };

            this.BeginAnimation(Window.OpacityProperty, opacityAnimation);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string employeeInitials = EmployeeInitialsTextBox.Text;
            string transactionPIN = TransactionPINTextBox.Password;

            EmployeeInitials = EmployeeInitialsTextBox.Text;
            TransactionPIN = TransactionPINTextBox.Password;

            if (ValidateCredentials(employeeInitials, transactionPIN))
            {
                this.DialogResult = true;
                EnteredEmployeeInitials = EmployeeInitialsTextBox.Text;
                EnteredTransactionPIN = TransactionPINTextBox.Password;
                this.Close();
            }
            else
            {
                InvalidCredentialsAlert invalidCredentialsAlert = new InvalidCredentialsAlert();
                invalidCredentialsAlert.ShowDialog();
            }
        }

        private bool ValidateCredentials(string employeeInitials, string transactionPIN)
        {
            string query = "SELECT EmployeeID, EmployeeFirstName FROM Employees WHERE EmployeeInitials = @EmployeeInitials AND EmployeeTransactionPIN = @EmployeeTransactionPIN";

            // Replace OleDbConnection with SqlConnection
            using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                // Replace OleDbCommand with SqlCommand
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Use SqlParameter to prevent SQL injection and set parameters for the query
                    command.Parameters.Add(new SqlParameter("@EmployeeInitials", employeeInitials));
                    command.Parameters.Add(new SqlParameter("@EmployeeTransactionPIN", transactionPIN));

                    try
                    {
                        connection.Open();
                        // Replace OleDbDataReader with SqlDataReader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                EmployeeID = reader["EmployeeID"].ToString();  // Adjust column name based on SQL table schema
                                EmployeeFirstName = reader["EmployeeFirstName"].ToString();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        // Handle exception (log or display a message)
                        MessageBox.Show("An error occurred while validating credentials: " + exception.Message);
                        return false;
                    }
                }
            }
        }

    }
}
