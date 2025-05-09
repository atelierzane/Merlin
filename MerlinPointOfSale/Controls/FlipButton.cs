using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MerlinPointOfSale.Controls
{
    public class FlipButton : Button
    {
        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register(nameof(RotationAngle), typeof(double), typeof(FlipButton),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private ScaleTransform _scaleTransform;

        public double RotationAngle
        {
            get => (double)GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, value);
        }

        public FlipButton()
        {
            // Ensure the ScaleTransform is mutable
            RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(1, 1),
                    new RotateTransform(0)
                }
            };
            RenderTransformOrigin = new Point(0.5, 0.5);

            _scaleTransform = ((TransformGroup)RenderTransform).Children[0] as ScaleTransform;
        }

        protected override void OnClick()
        {
            base.OnClick();

            // Create the flip animation
            var rotationAnimation = new DoubleAnimation
            {
                From = RotationAngle,
                To = RotationAngle + 180,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            BeginAnimation(RotationAngleProperty, rotationAnimation);

            // Scale up during the flip
            var scaleUp = new DoubleAnimation
            {
                To = 1.5,
                Duration = TimeSpan.FromMilliseconds(250),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            _scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
            _scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

            // Notify that the flip is complete to trigger the view transition
            rotationAnimation.Completed += (s, e) =>
            {
                OnFlipCompleted();
            };
        }

        public event RoutedEventHandler FlipCompleted;

        protected virtual void OnFlipCompleted()
        {
            FlipCompleted?.Invoke(this, new RoutedEventArgs());
        }
    }
}
