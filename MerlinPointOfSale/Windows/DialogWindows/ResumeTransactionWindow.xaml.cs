using MerlinPointOfSale.Models;
using MerlinPointOfSale.Style.Class;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class ResumeTransactionWindow : Window
    {
        private Brush originalBrush;
        public Transaction SelectedTransaction { get; private set; }

        public ResumeTransactionWindow(List<Transaction> suspendedTransactions)
        {
            InitializeComponent();
            SuspendedTransactionsDataGrid.ItemsSource = suspendedTransactions;
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
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);

            AdjustBorderGlow(mousePosition); // Border glow logic
            UpdateSeparatorGlow(mousePosition); // Glow separator logic
        }

        private void UpdateSeparatorGlow(Point mousePosition)
        {
            if (glowSeparator == null) return;

            double separatorY = glowSeparator.TransformToAncestor(this)
                .Transform(new Point(0, 0)).Y + glowSeparator.ActualHeight / 2;
            double distanceY = Math.Abs(mousePosition.Y - separatorY);

            double glowIntensity = Math.Max(0, 1 - (distanceY / 100));
            double relativeX = mousePosition.X / this.ActualWidth;

            ApplyGlowEffectToSeparator(glowIntensity, relativeX);
        }

        private void ApplyGlowEffectToSeparator(double intensity, double relativeX)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            double startOffset = Math.Max(0, relativeX - 0.1 * intensity);
            double endOffset = Math.Min(1, relativeX + 0.1 * intensity);

            brush.GradientStops.Add(new GradientStop(Colors.Transparent, startOffset));
            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(255 * intensity), 0, 198, 255), relativeX));
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, endOffset));

            glowSeparator.Fill = brush;
        }



        private void UpdateSeparatorGlowBG(Point mousePosition)
        {
            if (glowSeparatorBG == null) return;

            double separatorY = glowSeparatorBG.TransformToAncestor(this)
                .Transform(new Point(0, 0)).Y + glowSeparatorBG.ActualHeight / 2;
            double distanceY = Math.Abs(mousePosition.Y - separatorY);

            double glowIntensity = Math.Max(0, 1 - (distanceY / 100));
            double relativeX = mousePosition.X / this.ActualWidth;

            ApplyGlowEffectToSeparatorBG(glowIntensity, relativeX);
        }

        private void ApplyGlowEffectToSeparatorBG(double intensity, double relativeX)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };

            double startOffset = Math.Max(0, relativeX - 0.15 * intensity);
            double endOffset = Math.Min(1, relativeX + 0.15 * intensity);

            brush.GradientStops.Add(new GradientStop(Colors.Transparent, startOffset));
            brush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(255 * intensity), 0, 198, 255), relativeX));
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, endOffset));

            glowSeparatorBG.Fill = brush;
        }


        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetSeparatorGlow();
            ResetSeparatorBackgroundGlow();
        }

        private void ResetSeparatorGlow()
        {
            glowSeparator.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void ResetSeparatorBackgroundGlow()
        {
            glowSeparatorBG.Fill = new SolidColorBrush(Colors.Transparent);
        }


        private void Window_Activated(object sender, EventArgs e)
        {
            AnimateWindowOpacity(0.45, 1.0);
            AnimateBlurEffect(2.5, 0);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            AnimateWindowOpacity(1.0, 0.45);
            AnimateBlurEffect(0, 2.5);
        }

        private void AdjustBorderGlow(Point mousePosition)
        {
            double width = mainBorder.ActualWidth;
            double height = mainBorder.ActualHeight;

            double offsetX = mousePosition.X / width;
            double offsetY = mousePosition.Y / height;

            Color originalColor = Color.FromRgb(27, 73, 167);
            Color lighterColor = Color.FromRgb(0, 198, 255);

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

        private void FadeOutGlowEffect()
        {
            var brush = mainBorder.BorderBrush as RadialGradientBrush;
            if (brush != null)
            {
                Color originalColor = Color.FromRgb(27, 73, 167);

                ColorAnimation animation = new ColorAnimation
                {
                    To = originalColor,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                if (brush.GradientStops.Count > 0)
                {
                    brush.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, animation);
                }
            }
        }

        private void AnimateBlurEffect(double fromRadius, double toRadius)
        {
            BlurEffect blurEffect = new BlurEffect();
            this.Effect = blurEffect;

            DoubleAnimation blurAnimation = new DoubleAnimation
            {
                From = fromRadius,
                To = toRadius,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            blurEffect.BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
        }

        private void AnimateWindowOpacity(double from, double to)
        {
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            this.BeginAnimation(Window.OpacityProperty, opacityAnimation);
        }

        private void OnResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuspendedTransactionsDataGrid.SelectedItem is Transaction transaction)
            {
                SelectedTransaction = transaction;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a transaction to resume.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnCancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnCloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
