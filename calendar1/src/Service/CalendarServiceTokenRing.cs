using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using Microsoft.Samples.XmlRpc;

namespace BitCalendarService
{
    public class CalendarServiceTokenRing : ICalendarService
    {
        private CalendarDatabaseManager _calendarDatabaseManager    = new CalendarDatabaseManager();
        private static  Thread          _tokenThread                = new Thread(TokenCoroutine);
        private const   string          _baseAddress                = "/BitCalendarService";

        private static  bool            _requestingCriticalRegion   = false;
        private static  bool            _accessingCriticalRegion    = false;
        private static  bool            _hasToken                   = false;

        static CalendarServiceTokenRing()
        {
            
        }
        public CalendarServiceTokenRing()
        {
            
        }

        public static void TokenCoroutine()
        {
            while (true)
            {
                if (_hasToken)
                {
                    while (_requestingCriticalRegion || _accessingCriticalRegion)
                    {
                        Console.WriteLine("Passing Thread: Using token!");
                    }

                    Console.WriteLine("Passing thread: Token free! Passing.");

                    // Setup a channel for remote service.
                    _hasToken = false;
                    var remoteChannelFactory    = CreateChannelFactory(CalendarServiceUtility.IPAndPort);
                    var remoteCalendarService   = remoteChannelFactory.CreateChannel();
                    remoteCalendarService.PassToken();
                    remoteChannelFactory.Close();

                    Console.WriteLine("Passing thread: Token passing complete.");
                }
            }
        }
        public bool     Initialize()
        {
            if (!_tokenThread.IsAlive)
            {
                Console.WriteLine("Starting token thread.");
                _tokenThread.Start();
            }

            if (!DecodeUsers(GetUsers()).Any(x => x.IPAddress == CalendarServiceUtility.IPAndPort) && DecodeUsers(GetUsers()).Count() == 0)
                RegisterUser(CalendarServiceUtility.IPAndPort);
            
            Console.WriteLine("Initialization complete.");

            return true;
        }
        public bool     PassToken()
        {
            // We are done with token. Attempt passing it.
            _hasToken = false;

            // Get index of the next user.
            var users       = DecodeUsers(GetUsers()).ToList();
            var ownIndex    = users.IndexOf(users.Find(x => x.IPAddress == CalendarServiceUtility.IPAndPort));
            var tarIndex    = ownIndex + 1 == users.Count ? 0 : ownIndex + 1;

            Console.WriteLine("Initial target index is: " + tarIndex);

            // Pass it to the next available user.
            var result = false;
            while (!result)
            {
                try
                {
                    // Setup a channel for remote service.
                    var remoteChannelFactory    = CreateChannelFactory(users[tarIndex].IPAddress);
                    var remoteCalendarService   = remoteChannelFactory.CreateChannel();

                    Console.WriteLine("Passing token to: " + users[tarIndex].IPAddress);

                    // Attempt to make it receieve token.
                    remoteCalendarService.ReceiveToken();

                    Console.WriteLine("Passed token to: " + users[tarIndex].IPAddress);

                    result = true;
                }
                catch (Exception)
                {
                    result   = false;
                    tarIndex = tarIndex + 1 == users.Count ? 0 : tarIndex + 1;
                }
            }

            return result;
        }
        public bool     ReceiveToken()
        {
            Console.WriteLine("Received token!");

            _hasToken = true;

            return true;
        }

        public bool     CreateCalendarEvent(string calendarEventString)
        {
            _requestingCriticalRegion = true;

            // If we are connected and we don't have token, wait.
            if(DecodeUsers(GetUsers()).Length > 1)
                while (!_hasToken) ;

            _accessingCriticalRegion = true;

            var result = true; // CreateCalendarEventInternal(calendarEventString);

            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                var channelFactory  = CreateChannelFactory(user.IPAddress);
                var calendarService = channelFactory.CreateChannel();
                if (!calendarService.CreateCalendarEventInternal(calendarEventString))
                    result = false;
                channelFactory.Close();
            }

            _requestingCriticalRegion   = false;
            _accessingCriticalRegion    = false;

            return result;
        }
        public bool     EditCalendarEvent(string calendarEventString)
        {
            _requestingCriticalRegion = true;

            // If we are connected and while we don't have token.
            if (DecodeUsers(GetUsers()).Length > 1)
                while (!_hasToken) ;

            _accessingCriticalRegion = true;

            var result = true; // EditCalendarEventInternal(calendarEventString);

            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                var channelFactory  = CreateChannelFactory(user.IPAddress);
                var calendarService = channelFactory.CreateChannel();
                if (!calendarService.EditCalendarEventInternal(calendarEventString))
                {
                    Console.WriteLine("Edit failed. Attempting add.");
                    if (!calendarService.CreateCalendarEventInternal(calendarEventString))
                        result = false;
                }
                channelFactory.Close();
            }

            _requestingCriticalRegion   = false;
            _accessingCriticalRegion    = false;

            return result;
        }
        public bool     DeleteCalendarEvent(string calendarEventString)
        {
            _requestingCriticalRegion = true;

            // If we are connected and while we don't have token.
            if (DecodeUsers(GetUsers()).Length > 1)
                while (!_hasToken) ;

            _accessingCriticalRegion = true;

            var result = true; // DeleteCalendarEventInternal(calendarEventString);

            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                var channelFactory = CreateChannelFactory(user.IPAddress);
                var calendarService = channelFactory.CreateChannel();
                if (!calendarService.DeleteCalendarEventInternal(calendarEventString))
                result = false;
                channelFactory.Close();
            }

            _requestingCriticalRegion   = false;
            _accessingCriticalRegion    = false;

            return result;
        }
        public string   GetCalendarEvents()
        {
            var calendarEventsString = "";

            var calendarEvents = _calendarDatabaseManager.GetCalendarEvents();
            var calendarEventStrings = new string[calendarEvents.Length];
            for (var i = 0; i < calendarEvents.Length; i++)
            {
                calendarEventStrings[i] = calendarEvents[i].EncodeToString();
                calendarEventsString += calendarEventStrings[i];
                if (i + 1 < calendarEvents.Length)
                    calendarEventsString += "---";
            }

            return calendarEventsString;
        }
        public string   GetUsers()
        {
            var calendarUsersString = "";

            var calendarUsers = _calendarDatabaseManager.GetUsers();
            var calendarUserStrings = new string[calendarUsers.Length];
            for (var i = 0; i < calendarUsers.Length; i++)
            {
                calendarUserStrings[i] = calendarUsers[i].EncodeToString();
                calendarUsersString += calendarUserStrings[i];
                if (i + 1 < calendarUsers.Length)
                    calendarUsersString += "---";
            }
            return calendarUsersString;
        }

        public bool     Connect(string targetIPAddress)
        {
            bool result;

            Console.WriteLine("Attempting to connect to IP: " + targetIPAddress);

            try
            {
                // Setup a channel for remote service.
                var remoteChannelFactory    = CreateChannelFactory(targetIPAddress);
                var remoteCalendarService   = remoteChannelFactory.CreateChannel();

                Console.WriteLine("Retrieving users list.");

                // Sync users from the target PC (add yourself too for token ring).
                var usersString = remoteCalendarService.GetUsers() + "---" + CalendarServiceUtility.IPAndPort;

                Console.WriteLine("Retrieved users list.");
                Console.WriteLine("Syncing users.");

                result = SyncUsers(usersString);

                Console.WriteLine("Synced users.");
                Console.WriteLine("Syncing calendar events.");

                if(result)
                    result = SyncCalendarEvents(remoteCalendarService.GetCalendarEvents());

                Console.WriteLine("Synced calendar events.");

                remoteChannelFactory.Close();

                // Register to other people's user table.
                var users = DecodeUsers(GetUsers());
                foreach (var user in users)
                {
                    if (user.IPAddress == CalendarServiceUtility.IPAndPort)
                        continue;

                    var othersChannelFactory    = CreateChannelFactory(user.IPAddress);
                    var othersCalendarService   = othersChannelFactory.CreateChannel();
                    if (!othersCalendarService.RegisterUser(CalendarServiceUtility.IPAndPort))
                        result = false;
                    othersChannelFactory.Close();
                }

                // If network consists of me and someone else (if network was just initiated):
                if (users.Length == 2) /* make this two */
                {
                    ReceiveToken();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception connecting to network from IP: " + targetIPAddress);
                return false;
            }

            return result;
        }
        public bool     Disconnect()
        {
            // You cannot disconnect until you pass the token.
            while (_hasToken) ;

            var result = true;

            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                var othersChannelFactory = CreateChannelFactory(user.IPAddress);
                var othersCalendarService = othersChannelFactory.CreateChannel();
                if (!othersCalendarService.DeregisterUser(CalendarServiceUtility.IPAndPort))
                    result = false;
                othersChannelFactory.Close();
            }

            ClearUsersTable();

            return result;
        }
        public bool     RegisterUser(string calendarUserIPString)
        {
            if (_calendarDatabaseManager.CreateUser(new CalendarUser(calendarUserIPString)))
                return true;
            return false;
        }
        public bool     DeregisterUser(string calendarUserIPString)
        {
            if (_calendarDatabaseManager.DeleteUser(new CalendarUser(calendarUserIPString)))
                return true;
            return false;
        }

        public bool     ClearCalendarEventsTable()
        {
            _calendarDatabaseManager.ClearCalendarEvents();
            return true;
        }
        public bool     ClearUsersTable()
        {
            _calendarDatabaseManager.ClearUsers();

            if (!DecodeUsers(GetUsers()).Any(x => x.IPAddress == CalendarServiceUtility.IPAndPort) && DecodeUsers(GetUsers()).Count() == 0)
                RegisterUser(CalendarServiceUtility.IPAndPort);

            return true;
        }

        public bool     CreateCalendarEventInternal(string calendarEventString)
        {
            var result = _calendarDatabaseManager.CreateCalendarEvent(new CalendarEvent(calendarEventString));
            return result;
        }
        public bool     EditCalendarEventInternal(string calendarEventString)
        {
            var result = _calendarDatabaseManager.EditCalendarEvent(new CalendarEvent(calendarEventString));
            return result;
        }
        public bool     DeleteCalendarEventInternal(string calendarEventString)
        {
            var result = _calendarDatabaseManager.DeleteCalendarEvent(new CalendarEvent(calendarEventString));
            return result;
        }

        public string   RequestLock(string timestampString)
        {
            throw new NotImplementedException();
        }
        public bool     DebugLock()
        {
            _requestingCriticalRegion = true;

            return true;
        }
        public bool     DebugUnlock()
        {
            _requestingCriticalRegion = false;

            return true;
        }

        private bool    SyncUsers(string calendarUsersString)
        {
            if (calendarUsersString == null)
                return true;

            string[] stringSeperators = new string[] { "---" };
            string[] splitList = calendarUsersString.Split(stringSeperators, StringSplitOptions.None);

            var calendarUsers = new List<CalendarUser>();
            for (var i = 0; i < splitList.Length; i++)
            {
                var calendarUser = new CalendarUser();
                if (calendarUser.DecodeFromString(splitList[i]))
                    calendarUsers.Add(calendarUser);
            }

            _calendarDatabaseManager.ClearUsers();
            _calendarDatabaseManager.CreateUsers(calendarUsers.ToArray());

            return true;
        }
        private bool    SyncCalendarEvents(string calendarEventsString)
        {
            if (calendarEventsString == null)
                return true;
            string[] stringSeperators = new string[] { "---" };
            string[] splitList = calendarEventsString.Split(stringSeperators, StringSplitOptions.None);

            var calendarEvents = new List<CalendarEvent>();
            for (var i = 0; i < splitList.Length; i++)
            {
                var calendarEvent = new CalendarEvent();
                if (calendarEvent.DecodeFromString(splitList[i]))
                    calendarEvents.Add(calendarEvent);
            }

            // Should we really delete all previous events when connecting?
            _calendarDatabaseManager.ClearCalendarEvents();
            _calendarDatabaseManager.CreateCalendarEvents(calendarEvents.ToArray());

            return true;
        }

        private CalendarUser[]                              DecodeUsers(string calendarUsersString)
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
        private CalendarEvent[]                             DecodeCalendarEvents(string calendarEventsString)
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
        private static ChannelFactory<ICalendarService>     CreateChannelFactory(string ipAndPort)
        {
            var address = new Uri("http://" + ipAndPort + Program.BaseAddress);
            var channelFactory =
                new ChannelFactory<ICalendarService>(new WebHttpBinding(WebHttpSecurityMode.None),
                                                     new EndpointAddress(address));
            channelFactory.Endpoint.Behaviors.Add(new XmlRpcEndpointBehavior());

            Console.WriteLine("Created channel factory to: " + address);

            return channelFactory;
        }
    }
}
