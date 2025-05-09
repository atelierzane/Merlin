using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Controls
{
    public class MenuRadioButton : RadioButton
    {
        public static readonly DependencyProperty ContentVisibilityProperty =
            DependencyProperty.Register("ContentVisibility", typeof(Visibility), typeof(MenuRadioButton), new PropertyMetadata(Visibility.Visible));

        public Visibility ContentVisibility
        {
            get { return (Visibility)GetValue(ContentVisibilityProperty); }
            set { SetValue(ContentVisibilityProperty, value); }
        }
    }
}
