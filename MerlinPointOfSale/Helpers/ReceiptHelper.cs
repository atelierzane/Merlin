using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MerlinPointOfSale.Properties;
using MerlinPointOfSale.Models;
using ZXing.Windows.Compatibility;
using ZXing;

namespace MerlinPointOfSale.Helpers
{
    class ReceiptHelper
    {
        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool ReadPrinter(IntPtr hPrinter, IntPtr pBuffer, int dwBytesToRead, out int dwBytesRead);

        public string GetPrinterName_Epson()
        {
            string printerName = "EPSON TM-T88V Receipt";
            return printerName;
        }

        public void PrintNoSaleDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            System.Drawing.Image logo = Properties.Resources.NGP_Receipt_Logo_Bottom;
            int receiptWidth = 270; // Approximate width in pixels for 80mm at 96 DPI, adjust as needed
            float scale = receiptWidth / (float)logo.Width;
            int logoHeight = (int)(logo.Height * scale);
            Graphics graphics = e.Graphics;

            int startX = 10;
            int startY = 235;
            int offset = 120;
            int columnOffset = 165; // Offset for the second column. Adjust as needed.

            // Draw the scaled logo
            graphics.DrawImage(logo, startX, startY, receiptWidth, logoHeight);
            // Example of printing a simple "No Sale" slip
            e.Graphics.DrawString("NO SALE", new System.Drawing.Font("Inter", 12, System.Drawing.FontStyle.Bold), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 0));
            e.Graphics.DrawString($"Date: {DateTime.Now.ToShortDateString()}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 35));
            e.Graphics.DrawString($"Time: {DateTime.Now.ToShortTimeString()}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 50));

            e.Graphics.DrawString($"Store: {Settings.Default.LocationID}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0 + columnOffset, 35));
            e.Graphics.DrawString($"Register: {Settings.Default.RegisterNumber}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0 + columnOffset, 50));

            e.Graphics.DrawString($"Reason: ____________________________________", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 80));
            e.Graphics.DrawString($"Manager Approval: ________________________", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 125));

            e.Graphics.DrawString($"Store this document in the corresponding \ntill until end of business day. File with \nclosing documents.", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 165));

            e.Graphics.DrawString(".", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.LightGray, new System.Drawing.Point(0, 345));
            // Add your logo or additional information as needed
        }

        public void PrintCashDropDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            System.Drawing.Image logo = Properties.Resources.NGP_Receipt_Logo_Bottom;
            int receiptWidth = 270; // Approximate width in pixels for 80mm at 96 DPI, adjust as needed
            float scale = receiptWidth / (float)logo.Width;
            int logoHeight = (int)(logo.Height * scale);
            Graphics graphics = e.Graphics;

            int startX = 10;
            int startY = 325;
            int offset = 120;
            int columnOffset = 165; // Offset for the second column. Adjust as needed.

            // Draw the scaled logo
            graphics.DrawImage(logo, startX, startY, receiptWidth, logoHeight);
            // Example of printing a simple "No Sale" slip
            e.Graphics.DrawString("CASH DROP", new System.Drawing.Font("Inter", 12, System.Drawing.FontStyle.Bold), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 0));
            e.Graphics.DrawString($"Date: {DateTime.Now.ToShortDateString()}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 35));
            e.Graphics.DrawString($"Time: {DateTime.Now.ToShortTimeString()}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 50));

            e.Graphics.DrawString($"Store: {Settings.Default.LocationID}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0 + columnOffset, 35));
            e.Graphics.DrawString($"Register: {Settings.Default.RegisterNumber}", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0 + columnOffset, 50));

            e.Graphics.DrawString($"Amount: ____________________________________", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 80));
            e.Graphics.DrawString($"Employee Initials: __________________________", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 125));
            e.Graphics.DrawString($"Manager Approval: _________________________", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 170));

            e.Graphics.DrawString($"Associates signed above have verified the \namount indicated for this cash drop or \nmidday deposit as accurate.", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 210));
            e.Graphics.DrawString($"Place this document in the cash drop \nenvelope and secure immediately in safe.", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.Black, new System.Drawing.Point(0, 275));

            e.Graphics.DrawString(".", new System.Drawing.Font("Inter", 10), System.Drawing.Brushes.LightGray, new System.Drawing.Point(0, 430));
            // Add your logo or additional information as needed
        }


        public void PrintStandardReceipt(Transaction transaction, List<TransactionItems> summaryItems, string employeeID, int transactionNumber, bool isReprint = true)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = GetPrinterName_Epson();

            printDocument.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;
                Font font = new Font("Inter", 10);
                Font boldFont = new Font("Inter", 10, FontStyle.Bold);

                float fontHeight = font.GetHeight();
                int startX = 10;
                int receiptWidth = 270;
                int offset = 10;

                // Business logo
                Bitmap optimizedLogo = OptimizeImageForPrinter(Properties.Resources.NGP_Receipt_Logo_WhiteBG);
                graphics.DrawImage(optimizedLogo, startX, offset, receiptWidth, optimizedLogo.Height);
                offset += optimizedLogo.Height + 10;

                // Reprint indicator
                if (isReprint)
                {
                    string reprintText = "*** REPRINT ***";
                    string notValidText = "*** NOT VALID FOR RETURNS ***";

                    graphics.DrawString(reprintText, boldFont, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(reprintText, boldFont).Width) / 2, offset);
                    offset += (int)fontHeight + 5;

                    graphics.DrawString(notValidText, boldFont, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(notValidText, boldFont).Width) / 2, offset);
                    offset += (int)fontHeight + 10;
                }

                // Top Section: Employee and Transaction Details
                graphics.DrawString($"Date: {transaction.TransactionDate.ToShortDateString()}", font, Brushes.Black, startX, offset);
                graphics.DrawString($"Transaction: # {transactionNumber}", font, Brushes.Black, startX, offset + 20);
                graphics.DrawString($"Associate: {employeeID}", font, Brushes.Black, startX, offset + 40);

                graphics.DrawString($"Time: {transaction.TransactionTime.ToShortTimeString()}", font, Brushes.Black, startX + 150, offset);
                graphics.DrawString($"Store: {Settings.Default.LocationID}", font, Brushes.Black, startX + 150, offset + 20);
                graphics.DrawString($"Register: {Settings.Default.RegisterNumber}", font, Brushes.Black, startX + 150, offset + 40);

                offset += 60;

                // Separator line
                graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
                offset += (int)fontHeight;

                // Print transaction items
                foreach (var item in summaryItems)
                {
                    string itemName = item.Description.Length > 34 ? item.Description.Substring(0, 34) + "..." : item.Description;
                    graphics.DrawString(itemName, font, Brushes.Black, startX, offset);
                    offset += (int)fontHeight;

                    string itemDetails = $"Qty: {item.Quantity} @ {item.Price:C}";
                    graphics.DrawString(itemDetails, font, Brushes.Black, startX, offset);
                    offset += (int)fontHeight + 10;
                }

                // Print totals
                graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset); // Separator line
                offset += (int)fontHeight;

                string[] labels = { "Subtotal:", "Taxes:", "Total:" };
                decimal[] amounts = { transaction.Subtotal, transaction.Taxes, transaction.TotalAmount };

                for (int i = 0; i < labels.Length; i++)
                {
                    string label = labels[i];
                    string amountText = $"{amounts[i]:C2}";
                    SizeF amountSize = graphics.MeasureString(amountText, boldFont);
                    int amountPosX = receiptWidth - (int)amountSize.Width - 10;

                    graphics.DrawString(label, boldFont, Brushes.Black, startX, offset);
                    graphics.DrawString(amountText, boldFont, Brushes.Black, amountPosX, offset);
                    offset += (int)fontHeight + 5;
                }

                // Footer: Loyalty Program or Disclaimer
                graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
                offset += (int)fontHeight;

                string disclaimerText = "This document has been generated for your personal records.  It is not valid proof of purchase for returns, refunds, or exchanges.";
                List<string> wrappedDisclaimer = WrapText(disclaimerText, receiptWidth, font, graphics);

                foreach (string line in wrappedDisclaimer)
                {
                    graphics.DrawString(line, font, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(line, font).Width) / 2, offset);
                    offset += (int)fontHeight + 2;
                }
            };

            printDocument.Print();
        }



        private void DelayForPrinter()
        {
            System.Threading.Thread.Sleep(100); // Adjust based on printer responsiveness
        }
        private List<string> WrapText(string text, int maxWidth, Font font, Graphics graphics)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');
            StringBuilder line = new StringBuilder();

            foreach (string word in words)
            {
                string testLine = line.Length == 0 ? word : line + " " + word;
                if (graphics.MeasureString(testLine, font).Width > maxWidth)
                {
                    lines.Add(line.ToString());
                    line.Clear();
                    line.Append(word);
                }
                else
                {
                    line.Append(line.Length == 0 ? word : " " + word);
                }
            }

            if (line.Length > 0)
            {
                lines.Add(line.ToString());
            }

            return lines;
        }

        private Bitmap OptimizeImageForPrinter(Bitmap original)
        {
            int targetWidth = 270; // Adjust for receipt width
            int targetHeight = (int)(original.Height * (targetWidth / (float)original.Width));
            return new Bitmap(original, new System.Drawing.Size(targetWidth, targetHeight));
        }

        public void PrintGiftReceiptPage(Graphics graphics, List<TransactionItems> selectedItems, string transactionId, int transactionNumber)
        {
            Font font = new Font("Inter", 10);
            Font boldFont = new Font("Inter", 10, FontStyle.Bold);

            float fontHeight = font.GetHeight();
            int startX = 10;
            int receiptWidth = 270; // Approximate receipt width
            int offset = 10;

            // Business logo
            Bitmap optimizedLogo = OptimizeImageForPrinter(Properties.Resources.NGP_Receipt_Logo_WhiteBG);
            graphics.DrawImage(optimizedLogo, startX, offset, receiptWidth, optimizedLogo.Height);
            offset += optimizedLogo.Height + 10;

            // Separator line
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += (int)fontHeight;

            // Transaction details
            graphics.DrawString($"Transaction: #{transactionNumber}", font, Brushes.Black, startX, offset);
            offset += (int)fontHeight + 5;

            // Gift receipt header
            graphics.DrawString("Gift Receipt Items:", boldFont, Brushes.Black, startX, offset);
            offset += (int)fontHeight + 5;

            // Print selected items
            foreach (var item in selectedItems)
            {
                string itemName = item.Description.Length > 34 ? item.Description.Substring(0, 34) + "..." : item.Description;
                graphics.DrawString(itemName, font, Brushes.Black, startX, offset);
                offset += (int)fontHeight + 5;
            }

            // Separator line
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += (int)fontHeight;

            // QR Code for TransactionID
            Bitmap qrCodeImage = GenerateQRCode(transactionId);
            int qrCodeHeight = qrCodeImage.Height;
            int centeredX = startX + (receiptWidth - qrCodeImage.Width) / 2;
            graphics.DrawImage(qrCodeImage, centeredX, offset);
            offset += qrCodeHeight + 10;

            // Footer
            string footerText = "Thank you for your purchase!";
            graphics.DrawString(footerText, boldFont, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(footerText, boldFont).Width) / 2, offset);
        }




        private Bitmap GenerateQRCode(string content)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 100,
                    Width = 100,
                    Margin = 1  // Adjust the margin as needed
                },
                Renderer = new BitmapRenderer()

            };
            return writer.Write(content);
        }

    }
}