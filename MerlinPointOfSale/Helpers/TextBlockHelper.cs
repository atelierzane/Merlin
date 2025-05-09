using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Helpers
{
    public static class TextBlockHelper
    {
        public static readonly DependencyProperty CharacterSpacingProperty =
            DependencyProperty.RegisterAttached(
                "CharacterSpacing",
                typeof(double),
                typeof(TextBlockHelper),
                new PropertyMetadata(0.0, OnCharacterSpacingChanged));

        public static double GetCharacterSpacing(TextBlock textBlock) =>
            (double)textBlock.GetValue(CharacterSpacingProperty);

        public static void SetCharacterSpacing(TextBlock textBlock, double value) =>
            textBlock.SetValue(CharacterSpacingProperty, value);

        private static void OnCharacterSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                var spacing = (double)e.NewValue;
                ApplyCharacterSpacing(textBlock, spacing);
            }
        }

        private static void ApplyCharacterSpacing(TextBlock textBlock, double spacing)
        {
            if (textBlock.Text == null)
                return;

            // Inject additional spacing
            var spacedText = string.Join(new string(' ', (int)spacing), textBlock.Text.ToCharArray());
            textBlock.Text = spacedText;
        }
    }
}
