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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MerlinPointOfSale.Models;
using Stripe;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for CardRefundWindow.xaml
    /// </summary>
    public partial class CardRefundWindow : Window
    {
        private PaymentServiceClient _paymentServiceClient;
        public event Action<string, string, decimal> RefundCompleted;  // New Event
        public CardRefundWindow(string chargeId, decimal refundAmount)
        {
            InitializeComponent();
            InitializeComponent();
            _paymentServiceClient = new PaymentServiceClient("https://www.newgameplus.co/");
            ChargeIdInput.Text = chargeId; // Set the charge ID in the input field
            RefundAmountInput.Text = (refundAmount * 100).ToString("F0"); // Convert to cents and format without decimal places
        }

        private async void ProcessRefund_Click(object sender, RoutedEventArgs e)
        {
            string chargeId = ChargeIdInput.Text;
            if (!long.TryParse(RefundAmountInput.Text, out long amountInCents))
            {
                MessageBox.Show("Invalid refund amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                MessageBox.Show($"Initiating refund for ChargeID: {chargeId}, Amount: {amountInCents} cents", "Processing Refund", MessageBoxButton.OK, MessageBoxImage.Information);

                var refundResponse = await _paymentServiceClient.CreateRefundAsync(chargeId, amountInCents);

                if (refundResponse != null)
                {


                    RefundCompleted?.Invoke(chargeId, refundResponse.RefundId, amountInCents / 100m);  // Invoke Event

                    MessageBox.Show($"Refund processed successfully: Status - {refundResponse.Status}, Refund ID - {refundResponse.RefundId}, Charge ID - {refundResponse.ChargeId}", "Refund Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusMessage.Text = "Refund processed successfully: " + refundResponse.Status;

                    this.Close();  // Close the refund window after success
                }
                else
                {
                    StatusMessage.Text = "Refund response is null.";
                    MessageBox.Show("Refund response is null.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing refund: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage.Text = "Error processing refund: " + ex.Message;
            }
        }
    }
}
