using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MerlinPointOfSale.Controls
{
    public partial class MenuBarControl : UserControl
    {
        private Button currentlyActiveButton = null;

        public event EventHandler<string> ButtonClicked;

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register(nameof(MenuItems), typeof(ObservableCollection<string>), typeof(MenuBarControl),
                new PropertyMetadata(new ObservableCollection<string>(), OnMenuItemsChanged));

        public ObservableCollection<string> MenuItems
        {
            get => (ObservableCollection<string>)GetValue(MenuItemsProperty);
            set => SetValue(MenuItemsProperty, value);
        }

        public MenuBarControl()
        {
            InitializeComponent();
            DataContext = this;

            // Subscribe to the Loaded event
            Loaded += MenuBarControl_Loaded;
        }

        private static void OnMenuItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuBarControl control)
            {
                control.GenerateButtons();
            }
        }

        private void MenuBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Activate the first button when the control is loaded
            if (ButtonPanel.Children.Count > 0 && ButtonPanel.Children[0] is Button firstButton)
            {
                AnimateIndicator(firstButton);
                currentlyActiveButton = firstButton;
                AdjustButtonOpacities(firstButton);

                // Trigger the ButtonClicked event for the first button
                ButtonClicked?.Invoke(this, firstButton.Content.ToString());
            }
        }

        private void GenerateButtons()
        {
            ButtonPanel.Children.Clear();

            foreach (var menuItem in MenuItems)
            {
                var button = new Button
                {
                    Content = menuItem,
                    FontSize = 22,
                    Style = (System.Windows.Style)FindResource("subMenuBarButton"),
                    Margin = new Thickness(0, 0, 20, 0)
                };
                button.Click += Button_Click;
                ButtonPanel.Children.Add(button);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                AnimateIndicator(clickedButton);
                currentlyActiveButton = clickedButton;
                AdjustButtonOpacities(clickedButton);

                ButtonClicked?.Invoke(this, clickedButton.Content.ToString());
            }
        }

        private void AnimateIndicator(Button selectedButton)
        {
            var targetX = selectedButton.TransformToVisual(CanvasContainer).Transform(new Point(0, 0)).X;
            var targetWidth = selectedButton.ActualWidth;

            var moveAnimation = new DoubleAnimation(targetX, TimeSpan.FromSeconds(0.15))
            {
                EasingFunction = new QuadraticEase()
            };
            IndicatorTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);

            var widthAnimation = new DoubleAnimation(targetWidth, TimeSpan.FromSeconds(0.15))
            {
                EasingFunction = new QuadraticEase()
            };
            Indicator.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
        }

        private void AdjustButtonOpacities(Button activeButton)
        {
            foreach (var child in ButtonPanel.Children)
            {
                if (child is Button button)
                {
                    button.Opacity = button == activeButton ? 1.0 : 0.5;
                }
            }
        }
    }
}
