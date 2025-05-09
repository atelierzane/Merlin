using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using MerlinPointOfSale.Style;

namespace MerlinPointOfSale.Controls
{
    public partial class SubMenuBarControl : UserControl
    {
        private Button currentlyActiveButton = null;

        public event EventHandler<string> ButtonClicked;



        public static readonly DependencyProperty SubMenuItemsProperty =
            DependencyProperty.Register(nameof(SubMenuItems), typeof(ObservableCollection<string>), typeof(SubMenuBarControl),
                new PropertyMetadata(new ObservableCollection<string>(), OnSubMenuItemsChanged));

        public string SelectedOption { get;  set; }

        public ObservableCollection<string> SubMenuItems
        {
            get => (ObservableCollection<string>)GetValue(SubMenuItemsProperty);
            set => SetValue(SubMenuItemsProperty, value);
        }

        public SubMenuBarControl()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += SubMenuBarControl_Loaded;
        }

        private static void OnSubMenuItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SubMenuBarControl control)
            {
                control.GenerateButtons();
            }
        }

        private void SubMenuBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ButtonPanel.Children.Count > 0 && ButtonPanel.Children[0] is Button firstButton)
            {
                AnimateIndicator(firstButton);
                currentlyActiveButton = firstButton;
                AdjustButtonOpacities(firstButton);
                SelectedOption = firstButton.Content.ToString();
                ButtonClicked?.Invoke(this, firstButton.Content.ToString());
            }
        }

        private void GenerateButtons()
        {
            ButtonPanel.Children.Clear();

            foreach (var menuItem in SubMenuItems)
            {
                var button = new Button
                {
                    Content = menuItem,
                    FontSize = 18, // Adjusted for SubMenuBar
                    Style = (System.Windows.Style)FindResource("subMenuBarButton2"), // Ensure this resource exists in your application
                    Margin = new Thickness(0, 0, 15, 0) // Adjusted for SubMenuBar
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
