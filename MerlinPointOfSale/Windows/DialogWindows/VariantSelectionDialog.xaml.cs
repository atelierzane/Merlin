using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MerlinPointOfSale.Controls;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using MerlinPointOfSale.Models;
using System.Windows.Media.Animation;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for VariantSelectionDialogWindow.xaml
    /// </summary>
    public partial class VariantSelectionDialog : Window
    {
        public InventoryItem SelectedVariant { get; private set; }
        private readonly string baseSKU;
        private readonly string connectionString;
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;
        private ApplicationHelper applicationHelper;

        public VariantSelectionDialog(string baseSKU, string connectionString)
        {
            InitializeComponent();
            this.baseSKU = baseSKU;
            this.connectionString = connectionString;

            LoadVariants();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            // Initialize VisualEffectsHelper
            visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
            inputHelper = new InputHelper(this, visualEffectsHelper);
            applicationHelper = new ApplicationHelper();
            // Play the custom notification sound
            applicationHelper.PlayCustomNotificationSound("MerlinPointOfSale.Resources.MerlinNotification1.wav");

            // Apply the Acrylic Blur Effect
            var blurEffect = new WindowBlurEffect(this) { BlurOpacity = 0.85 };

            // Trigger the border glow effect on window load
            visualEffectsHelper.AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));


            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top - 15,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.55),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            // Optional: Add a slight delay before starting content animations
            var contentGridAnimation = FindResource("DialogWindowAnimation") as Storyboard;
            if (contentGridAnimation != null)
            {
                contentGridAnimation.Begin(this);
            }

            this.BeginAnimation(Window.TopProperty, topAnimation);
        }

        private void LoadVariants()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT Variant1Name, Variant1Properties, Variant2Name, Variant2Properties, Variant3Name, Variant3Properties
                FROM Catalog
                WHERE SKU = @BaseSKU AND IsBaseSKU = 1"; // Ensure the BaseSKU is explicitly fetched

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BaseSKU", baseSKU);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate the combo boxes dynamically based on available variants
                                PopulateVariantComboBox(Variant1Panel, Variant1Label, Variant1ComboBox, reader["Variant1Name"].ToString(), reader["Variant1Properties"].ToString());
                                PopulateVariantComboBox(Variant2Panel, Variant2Label, Variant2ComboBox, reader["Variant2Name"].ToString(), reader["Variant2Properties"].ToString());
                                PopulateVariantComboBox(Variant3Panel, Variant3Label, Variant3ComboBox, reader["Variant3Name"].ToString(), reader["Variant3Properties"].ToString());
                            }
                            else
                            {
                                MessageBox.Show("No variants found for this Base SKU.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading variants: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void PopulateVariantComboBox(StackPanel panel, TextBlock label, ComboBox comboBox, string variantName, string variantProperties)
        {
            if (!string.IsNullOrEmpty(variantName) && !string.IsNullOrEmpty(variantProperties))
            {
                // Set visibility for the corresponding variant panel
                panel.Visibility = Visibility.Visible;
                label.Text = $"{variantName}:"; // Update label text

                var properties = variantProperties.Split(',');
                foreach (var property in properties)
                {
                    comboBox.Items.Add(property.Trim()); // Add each property to the combo box
                }
            }
            else
            {
                panel.Visibility = Visibility.Collapsed; // Hide the panel if no variants are available
            }
        }



        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT TOP 1 *
                FROM Catalog
                WHERE VariantAssignedToBaseSKU = @BaseSKU
                  AND (@Variant1Name IS NULL OR Variant1Properties = @Variant1Name)
                  AND (@Variant2Name IS NULL OR Variant2Properties = @Variant2Name)
                  AND (@Variant3Name IS NULL OR Variant3Properties = @Variant3Name)
                  AND IsVariantSKU = 1"; // Ensure only variants are matched

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BaseSKU", baseSKU);
                        cmd.Parameters.AddWithValue("@Variant1Name", Variant1ComboBox.SelectedItem?.ToString() ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Variant2Name", Variant2ComboBox.SelectedItem?.ToString() ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Variant3Name", Variant3ComboBox.SelectedItem?.ToString() ?? (object)DBNull.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                SelectedVariant = new InventoryItem
                                {
                                    SKU = reader["SKU"].ToString(),
                                    ProductName = reader["ProductName"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    CategoryID = reader["CategoryID"].ToString(),
                                    IsBaseSKU = false // Ensure it's treated as a variant
                                };
                            }
                        }
                    }
                }

                if (SelectedVariant != null)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("No matching variant found. Please adjust your selections.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming variant: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}