using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using BitCalendarService;
using Microsoft.Samples.XmlRpc;

// TODO: Add try-catch statements to every server call.
namespace BitCalendar.Core.Network
{
    class CalendarNetworkManager
    {
        public  string              IP
        {
            get
            {
                var result = "?";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        result = ip.ToString();
                        // break; - this causes incompatibility with Java sometimes!
                    }
                }
                return result;

                /*
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress addr in localIPs)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return addr.ToString();
                    }
                }
                return Environment.MachineName;
                */

            }
        }

        public static   int         Port            = 8080;
        public const    string      _baseAddress    = "/BitCalendarService";

        private ICalendarService    _localService;

        public CalendarNetworkManager()
        {
            _localService = CreateChannelFactory(IP + ":" + Port).CreateChannel();
            _localService.Initialize();
        }

        // Local & remote.
        public bool CreateCalendarEvent(CalendarEvent calendarEvent)
        {
            return _localService.CreateCalendarEvent(calendarEvent.EncodeToString());
        }
        public bool EditCalendarEvent(CalendarEvent calendarEvent)
        {
            return _localService.EditCalendarEvent(calendarEvent.EncodeToString());
        }
        public bool DeleteCalendarEvent(CalendarEvent calendarEvent)
        {
            return _localService.DeleteCalendarEvent(calendarEvent.EncodeToString());
        }
        public bool JoinNetwork(string targetIPAddress)
        {
            return _localService.Connect(targetIPAddress);
        }
        public bool LeaveNetwork()
        {
            return _localService.Disconnect();
        }
        // Local.
        public CalendarEvent[]  GetCalendarEvents()
        {
            var calendarEventsString = _localService.GetCalendarEvents();
            return DecodeCalendarEvents(calendarEventsString);
        }
        public CalendarUser[]   GetUsers()
        {
            var calendarUsersString = _localService.GetUsers();
            return DecodeUsers(calendarUsersString);
        }
        public bool             ClearUsersTable()
        {
            return _localService.ClearUsersTable();
        }
        public bool             ClearCalendarEventsTable()
        {
            return _localService.ClearCalendarEventsTable();
        }

        public void Lock()
        {
            bool result = _localService.DebugLock();
        }
        public void Unlock()
        {
            bool result = _localService.DebugUnlock();
        }

        private CalendarUser[]                      DecodeUsers(string calendarUsersString)
        {
            if (calendarUsersString == null)
                return new CalendarUser[0];

            string[] stringSeperators = new string[] { "---" };
            string[] splitList = calendarUsersString.Split(stringSeperators, StringSplitOptions.None);

            var calendarUsers = new List<CalendarUser>();
            for (int i = 0; i < splitList.Length; i++)
            {
                var calendarUser = new CalendarUser();
                if (calendarUser.DecodeFromString(splitList[i]))
                    calendarUsers.Add(calendarUser);
            }

            return calendarUsers.ToArray();
        }
        private CalendarEvent[]                     DecodeCalendarEvents(string calendarEventsString)
        {
            if (calendarEventsString == null)
                return new CalendarEvent[0];

            string[] stringSeperators = new string[] { "---" };
            string[] splitList = calendarEventsString.Split(stringSeperators, StringSplitOptions.None);

            var calendarEvents = new List<CalendarEvent>();
            for (int i = 0; i < splitList.Length; i++)
            {
                var calendarEvent = new CalendarEvent();
                if (!string.IsNullOrEmpty(splitList[i]) && calendarEvent.DecodeFromString(splitList[i]))
                    calendarEvents.Add(calendarEvent);
            }

            return calendarEvents.ToArray();
        }
        private ChannelFactory<ICalendarService> CreateChannelFactory(string ipAndPort)
        {
            var address = new Uri("http://" + ipAndPort + _baseAddress);
            var channelFactory =
                new ChannelFactory<ICalendarService>(new WebHttpBinding(WebHttpSecurityMode.None),
                                                     new EndpointAddress(address));
            channelFactory.Endpoint.Behaviors.Add(new XmlRpcEndpointBehavior());

            Console.WriteLine("Created channel factory to: " + address);

            return channelFactory;
        }
    }
}
