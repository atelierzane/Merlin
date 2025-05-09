using MerlinPointOfSale.Style.Class;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace MerlinPointOfSale.Helpers
{
    public class VisualEffectsHelper
    {
        private Window targetWindow;
        private Border mainBorder;
        private Canvas glowEffectCanvas;
        private Rectangle glowSeparator;
        private Rectangle glowSeparatorBG;

        public VisualEffectsHelper(Window window, Border border, Canvas glowCanvas, Rectangle separator, Rectangle separatorBG)
        {
            targetWindow = window;
            mainBorder = border;
            glowEffectCanvas = glowCanvas;
            glowSeparator = separator;
            glowSeparatorBG = separatorBG;

            targetWindow.StateChanged += OnWindowStateChanged;
            targetWindow.Loaded += OnWindowLoaded;
            targetWindow.MouseMove += OnMouseMove;
            targetWindow.MouseLeave += OnMouseLeave;
        }

        public void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (targetWindow.WindowState == WindowState.Minimized)
            {
                mainBorder.Margin = new Thickness(4);
            }
            else
            {
                mainBorder.Margin = new Thickness(0);
            }
        }

        public async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Start hidden
            targetWindow.Visibility = Visibility.Hidden;
            targetWindow.Opacity = 0;

            // Apply blur effect
            ApplyBlurEffect(1.0f);



            // Show the window and begin animations
            targetWindow.Visibility = Visibility.Visible;



            // Initialize glow and other effects
            AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));
        }




        public void ApplyBlurEffect(float blurOpacity)
        {
            var blurEffect = new WindowBlurEffect(targetWindow);
            blurEffect.EnableBlur();
        }

        public void AnimateBlurEffect(double fromRadius, double toRadius)
        {
            BlurEffect blurEffect = new BlurEffect();
            targetWindow.Effect = blurEffect;

            DoubleAnimation blurAnimation = new DoubleAnimation
            {
                From = fromRadius,
                To = toRadius,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
        }

        public void AnimateWindowOpacity(double from, double to)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            targetWindow.BeginAnimation(Window.OpacityProperty, opacityAnimation);
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(targetWindow);
            UpdateSeparatorGlow(mousePosition);
            UpdateSeparatorGlowBG(mousePosition);
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ResetSeparatorGlow();
            ResetSeparatorBackgroundGlow();
        }

        public void AdjustBorderGlow(Point mousePosition)
        {
            double width = mainBorder.ActualWidth;
            double height = mainBorder.ActualHeight;

            double offsetX = mousePosition.X / width;
            double offsetY = mousePosition.Y / height;

            // Retrieve colors from the resource dictionary
            Color originalColor = ((Color)Application.Current.Resources["windowBorderColor_merlinCyan_GradientStop1"]);
            Color lighterColor = ((Color)Application.Current.Resources["merlinCyanBorder_Highlight"]);

            RadialGradientBrush brush = new RadialGradientBrush
            {
                GradientOrigin = new Point(offsetX, offsetY),
                Center = new Point(offsetX, offsetY),
                RadiusX = 0.225,
                RadiusY = 0.225
            };

            brush.GradientStops.Add(new GradientStop(lighterColor, 0));
            brush.GradientStops.Add(new GradientStop(originalColor, 1));

            mainBorder.BorderBrush = brush;
        }



        private void UpdateSeparatorGlow(Point mousePosition)
        {
            double width = glowSeparator.ActualWidth;
            double offsetX = mousePosition.X / width;
            // Calculate the Y distance to determine glow intensity.
            double separatorYRelativeToWindow = glowSeparator.TransformToAncestor(targetWindow).Transform(new Point(0, 0)).Y + glowSeparator.ActualHeight / 2;
            double distanceYToSeparator = Math.Abs(mousePosition.Y - separatorYRelativeToWindow);
            double glowIntensity = Math.Max(0, 1 - (distanceYToSeparator / 250)); // Adjust this calculation as needed.

            // Calculate the X position for the glow effect to "follow" the mouse.
            double relativeXPosition = mousePosition.X / width;

            ApplyGlowEffectToSeparator(glowIntensity, relativeXPosition);
        }

        private void UpdateSeparatorGlowBG(Point mousePosition)
        {
            double width = glowSeparator.ActualWidth;
            double offsetX = mousePosition.X / width;
            // Calculate the Y distance to determine glow intensity.
            double separatorYRelativeToWindow = glowSeparatorBG.TransformToAncestor(targetWindow).Transform(new Point(0, 0)).Y + glowSeparatorBG.ActualHeight / 2;
            double distanceYToSeparator = Math.Abs(mousePosition.Y - separatorYRelativeToWindow);
            double glowIntensity = Math.Max(0, 1 - (distanceYToSeparator / 250)); // Adjust this calculation as needed.

            // Calculate the X position for the glow effect to "follow" the mouse.
            double relativeXPosition = mousePosition.X / width;

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
            glowSeparator.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void ResetSeparatorBackgroundGlow()
        {
            glowSeparatorBG.Fill = new SolidColorBrush(Colors.Transparent);
        }

        public void FadeOutGlowEffect()
        {
            var brush = mainBorder.BorderBrush as RadialGradientBrush;
            if (brush != null)
            {
                // Retrieve original colors from the resource dictionary
                Color originalStartColor = (Color)Application.Current.Resources["merlinBlue"];
                Color originalEndColor = (Color)Application.Current.Resources["merlinBlue"];

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

    }
}
