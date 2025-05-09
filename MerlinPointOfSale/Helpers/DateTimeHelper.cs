using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Helpers
{
    internal class DateTimeHelper
    {
        public static DateTime GetNetworkTime(string ntpServer = "pool.ntp.org")
        {
            const int ntpDataLength = 48;
            var ntpData = new byte[ntpDataLength];

            // Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; // LI = 0, VN = 3, Mode = 3 (client)

            var addresses = System.Net.Dns.GetHostEntry(ntpServer).AddressList;
            var endPoint = new IPEndPoint(addresses[0], 123);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(endPoint);
                socket.ReceiveTimeout = 3000; // 3 seconds timeout
                socket.Send(ntpData);
                socket.Receive(ntpData);
            }

            // Convert NTP time to DateTime
            const byte serverReplyTime = 40;
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            ulong fracPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            intPart = SwapEndianness(intPart);
            fracPart = SwapEndianness(fracPart);

            var milliseconds = (intPart * 1000) + ((fracPart * 1000) / 0x100000000L);
            var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) + ((x & 0x0000ff00) << 8) + ((x & 0x00ff0000) >> 8) + ((x & 0xff000000) >> 24));
        }
    }
}
