using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Windows;

namespace MerlinCommunicator.Helpers
{
    class ApplicationHelper
    {
        public string GetMerlinPollingAgentPath()
        {
            string path = @"C:\Program Files (x86)\zane.farnsworth@outlook.com\MerlinSuite_Setup\MerlinSuite_PollingAgent.exe";

            return path;
        }



        public string GetMerlinBackOfficePath()
        {
            string path = @"C:\Program Files (x86)\zane.farnsworth@outlook.com\MerlinSuite_Setup\MerlinSuite_BackOffice.exe";
            return path;
        }


        public string GetMerlinPollingAgentPath_Release()
        {
            string path = @"C:\Users\zanef\source\repos\MerlinSuite_PollingAgent\MerlinSuite_PollingAgent\bin\Debug\net8.0-windows\MerlinSuite_PollingAgent.exe";
            return path;
        }
        public string GetMerlinBackOfficePath_Release()
        {
            string path = @"C:\Users\zanef\source\repos\MerlinSuite_BackOffice\MerlinSuite_BackOffice\bin\Debug\net8.0-windows\MerlinSuite_BackOffice.exe";
            return path;
        }

        public string GetMerlinPointofSalePath_Release()
        {
            string path = @"C:\Users\zanef\source\repos\MerlinSuite_BackOffice\MerlinSuite_BackOffice\bin\Debug\net8.0-windows\MerlinSuite_PointOfSale.exe";
            return path;
        }

        public void PlayCustomNotificationSound(string resourceName)
        {
            try
            {
                // Get the current assembly
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                // Load the sound from the embedded resources
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception("Sound file not found in embedded resources.");
                    }

                    SoundPlayer player = new SoundPlayer(stream);
                    player.Load();  // Load the sound synchronously
                    player.Play();  // Play the sound
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing sound: " + ex.Message, "Sound Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
