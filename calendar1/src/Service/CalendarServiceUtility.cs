using System;
using System.Net;
using System.Net.Sockets;

namespace BitCalendarService
{
    class CalendarServiceUtility
    {
        public static string IP
        {
            get
            {
                var result = "?";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        result = ip.ToString();
                }
                return result;
            }
        }

        public static string IPAndPort
        {
            get { return IP + ":" + Program.Port; }
        }

    }
}
