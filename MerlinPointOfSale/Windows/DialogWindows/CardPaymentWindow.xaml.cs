using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using System.IO;
using MerlinPointOfSale.Style.Class;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class CardPaymentWindow : Window
    {
        private VisualHost visualHost = new VisualHost();
        private DrawingVisual glowEffectVisual = new DrawingVisual();
        private Brush originalBrush;
        public decimal ChargeAmount { get; set; }
        private bool isProcessingPayment = false;
        private PaymentServiceClient _paymentServiceClient;
        private HubConnection _hubConnection;
        private TaskCompletionSource<bool> _paymentCompletionSource;

        public event EventHandler<(string status, string cardLastFour, string cardBrand, int expMonth, int expYear, string chargeId)> PaymentCompleted;

        public CardPaymentWindow(decimal totalDue)
        {
            InitializeComponent();
            ChargeAmount = totalDue;
            _paymentServiceClient = new PaymentServiceClient("https://www.newgameplus.co/");
            InitializeSignalRAsync();
            InitializeAsync();
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

            // Start window animation
            var storyboard = this.FindResource("WindowAnimation_Legacy") as Storyboard;
            storyboard?.Begin();

            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top + 50,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(Window.TopProperty, topAnimation);

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

            // Define the original and lighter colors
            Color originalColor = Color.FromRgb(27, 73, 167); // Original blue color
            Color lighterColor = Color.FromRgb(0, 198, 255); // Original blue color

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
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 0);

            // Calculate the offsets for the glow effect to be centered around the mouse's X position.
            double startOffset = Math.Max(0, relativeXPosition - 0.1 * intensity); // Adjust these values as needed.
            double endOffset = Math.Min(1, relativeXPosition + 0.1 * intensity);

            brush.GradientStops.Add(new GradientStop(Colors.Transparent, startOffset));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * intensity), 0, 198, 255), relativeXPosition)); // Glow color at mouse position
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, endOffset));

            glowSeparator.Fill = brush;
        }

        private void ApplyGlowEffectToSeparatorBG(double intensity, double relativeXPosition)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 0);

            // Calculate the offsets for the glow effect to be centered around the mouse's X position.
            double startOffset = Math.Max(0, relativeXPosition - 0.15 * intensity); // Adjust these values as needed.
            double endOffset = Math.Min(1, relativeXPosition + 0.15 * intensity);

            brush.GradientStops.Add(new GradientStop(Colors.Transparent, startOffset));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * intensity), 0, 198, 255), relativeXPosition)); // Glow color at mouse position
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

        private async void InitializeAsync()
        {
            try
            {
                // Define the path for the WebView2 user data folder
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MerlinSuite_PointOfSale", "WebView2");

                // Ensure the directory exists
                if (!Directory.Exists(userDataFolder))
                {
                    Directory.CreateDirectory(userDataFolder);
                }

                // Create WebView2 environment with the custom data folder
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

                // Ensure WebView2 is initialized with the specified environment
                await webView.EnsureCoreWebView2Async(env);

                // Proceed with the payment processing if not already in progress
                if (!isProcessingPayment)
                {
                    isProcessingPayment = true;
                    ProcessPaymentAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}\n\n{ex.StackTrace}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void InitializeSignalRAsync()
        {
            _hubConnection = new HubConnectionBuilder().WithUrl("https://www.newgameplus.co/paymentUpdatesHub").Build();
            _hubConnection.On<string, string, string, string, int, int, string>("ReceiveChargeStatus", HandleChargeStatus);
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to SignalR hub: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleChargeStatus(string transactionId, string status, string cardLastFour, string cardBrand, int expMonth, int expYear, string chargeId)
        {
            Dispatcher.Invoke(() =>
            {
                if (status == "succeeded")
                {
                    PaymentCompleted?.Invoke(this, (status, cardLastFour, cardBrand, expMonth, expYear, chargeId));
                    _paymentCompletionSource.SetResult(true);
                }
                else
                {
                    MessageBox.Show("Payment failed. Please try again.", "Payment Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    _paymentCompletionSource.SetResult(false);
                }
            });
        }

        private async void ProcessPaymentAsync()
        {
            try
            {
                var paymentIntentResponse = await _paymentServiceClient.CreatePaymentIntentAsync((long)(ChargeAmount * 100), "usd");
                if (paymentIntentResponse != null)
                {
                    NavigateToPaymentPage(paymentIntentResponse.ClientSecret);
                    _paymentCompletionSource = new TaskCompletionSource<bool>();
                    var result = await _paymentCompletionSource.Task;
                    this.DialogResult = result;
                }
                else
                {
                    MessageBox.Show("Failed to create PaymentIntent.", "Creation Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating PaymentIntent: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
        }

        private void NavigateToPaymentPage(string clientSecret)
        {
            string baseUrl = "https://www.newgameplus.co/terminal/index.html";
            string query = $"?amount={ChargeAmount * 100}&currency=usd&clientSecret={clientSecret}";
            string fullUrl = $"{baseUrl}{query}";
            webView.CoreWebView2.Navigate(fullUrl);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_hubConnection != null)
            {
                _hubConnection.StopAsync();
                _hubConnection.DisposeAsync();
            }
            base.OnClosing(e);
        }
    }
}
