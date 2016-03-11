using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using Microsoft.Samples.XmlRpc;

namespace BitCalendarService
{
    // Note: Static variables persist between sessions!
    public class CalendarServiceRicartAgrawala : ICalendarService
    {
        private CalendarDatabaseManager _calendarDatabaseManager    = new CalendarDatabaseManager();
        
        // Number -1 corresponds to "add" critical region, otherwise region no is unique ID.
        private static bool             _requestingCriticalRegion   = false;
        private static bool             _accessingCriticalRegion    = false;
        private static int              _criticalRegionNo           = -1;

        private static LamportClock     _lamportClock               = new LamportClock();

        public bool     CreateCalendarEvent(string calendarEventString)
        {
            _criticalRegionNo           = -1;
            _requestingCriticalRegion   = true;

            // Request lock.
            QueryLock();

            // Start accessing critical region.
            _accessingCriticalRegion    = true;

            _lamportClock.Increment();

            var result = CreateCalendarEventInternal(calendarEventString);

            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                _lamportClock.Increment();

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
            _criticalRegionNo           = (new CalendarEvent(calendarEventString)).UniqueID;
            _requestingCriticalRegion   = true;

            QueryLock();

            _accessingCriticalRegion = true;
            
            _lamportClock.Increment();

            var result = true;
            if (!EditCalendarEventInternal(calendarEventString))
            {
                Console.WriteLine("Edit failed. Attempting add.");
                if (!CreateCalendarEventInternal(calendarEventString))
                    result = false;
            }

            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                _lamportClock.Increment();

                var channelFactory  = CreateChannelFactory(user.IPAddress);
                var calendarService = channelFactory.CreateChannel();
                if (!calendarService.EditCalendarEventInternal(calendarEventString))
                {
                    Console.WriteLine("Edit failed. Attempting add.");
                    if (!CreateCalendarEventInternal(calendarEventString))
                        result = false;
                }
                channelFactory.Close();
            }

            _requestingCriticalRegion = false;
            _accessingCriticalRegion = false;

            return result;
        }
        public bool     DeleteCalendarEvent(string calendarEventString)
        {
            _criticalRegionNo = (new CalendarEvent(calendarEventString)).UniqueID;
            _requestingCriticalRegion = true;

            QueryLock();

            _accessingCriticalRegion = true;

            _lamportClock.Increment();

            var result = DeleteCalendarEventInternal(calendarEventString);

            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                _lamportClock.Increment();

                var channelFactory = CreateChannelFactory(user.IPAddress);
                var calendarService = channelFactory.CreateChannel();
                if (!calendarService.DeleteCalendarEventInternal(calendarEventString))
                    result = false;
                channelFactory.Close();
            }

            _requestingCriticalRegion = false;
            _accessingCriticalRegion = false;

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

        public bool     Connect(string targetIPAndPort)
        {
            bool result;

            Console.WriteLine("Attempting to connect to IP: " + targetIPAndPort);

            try
            {
                // Setup a channel for remote service.
                var remoteChannelFactory = CreateChannelFactory(targetIPAndPort);
                var remoteCalendarService   = remoteChannelFactory.CreateChannel();

                Console.WriteLine("Retrieving users list.");

                // Sync users from the target PC.
                var usersString = remoteCalendarService.GetUsers() + "---" + targetIPAndPort;

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
                    var othersChannelFactory    = CreateChannelFactory(user.IPAddress);
                    var othersCalendarService   = othersChannelFactory.CreateChannel();
                    if (!othersCalendarService.RegisterUser(CalendarServiceUtility.IPAndPort))
                        result = false;
                    othersChannelFactory.Close();
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Exception connecting to network from IP: " + targetIPAndPort);
                return false;
            }

            return result;
        }
        public bool     Disconnect()
        {
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
            return true;
        }

        public bool     Initialize()
        {
            return true;
        }
        public void     QueryLock()
        {
            var users = DecodeUsers(GetUsers());
            foreach (var user in users)
            {
                Console.WriteLine("Requesting create lock from: " + user.IPAddress);

                _lamportClock.Increment();

                var extendedTimestamp = new ExtendedTimestamp(_lamportClock.CurrentTime, CalendarServiceUtility.IPAndPort, _criticalRegionNo);

                var othersChannelFactory = CreateChannelFactory(user.IPAddress);
                var othersCalendarService = othersChannelFactory.CreateChannel();

                var othersTimestampString = othersCalendarService.RequestLock(extendedTimestamp.ToString());
                var othersTimestamp = ExtendedTimestamp.FromString(othersTimestampString);
                _lamportClock.Adjust(othersTimestamp.Time);

                othersChannelFactory.Close();

                Console.WriteLine("Response to create lock received from: " + user.IPAddress);
            }
            Console.WriteLine("All lock responses receieved! Proceeding to critical region.");
        }
        public string   RequestLock(string timestampString)
        {
            Console.WriteLine("Received lock request. Timestamp string: " + timestampString);

            var extTimestamp = ExtendedTimestamp.FromString(timestampString);

            if (_criticalRegionNo != extTimestamp.CriticalRegionNo)
            {
                
            }
            else if (!_accessingCriticalRegion && !_requestingCriticalRegion)
            {
                
            }
            else if (_accessingCriticalRegion)
            {
                // Queue loop.
                while (_accessingCriticalRegion)
                {
                    Console.WriteLine("Delaying response, accessing critical region.");
                }
            }
            else if (_requestingCriticalRegion)
            {
                if (_lamportClock.CurrentTime > extTimestamp.Time)
                {
                    
                }
                else
                {
                    // Queue loop.
                    while (_requestingCriticalRegion)
                    {
                        Console.WriteLine("Delaying response, requesting critical region.");
                    }
                }
            }

            Console.WriteLine("Returning lock request!");
            _lamportClock.Adjust(extTimestamp.Time);
            return new ExtendedTimestamp(_lamportClock.CurrentTime, CalendarServiceUtility.IPAndPort, extTimestamp.CriticalRegionNo).ToString();      
        }

        public bool     CreateCalendarEventInternal(string calendarEventString)
        {
            Console.WriteLine("Internal create event received.");
            var result = _calendarDatabaseManager.CreateCalendarEvent(new CalendarEvent(calendarEventString));
            return result;
        }
        public bool     EditCalendarEventInternal(string calendarEventString)
        {
            Console.WriteLine("Internal edit event received.");
            var result = _calendarDatabaseManager.EditCalendarEvent(new CalendarEvent(calendarEventString));
            return result;
        }
        public bool     DeleteCalendarEventInternal(string calendarEventString)
        {
            Console.WriteLine("Internal delete event received.");
            var result = _calendarDatabaseManager.DeleteCalendarEvent(new CalendarEvent(calendarEventString));
            return result;
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
        public bool     PassToken()
        {
            throw new NotImplementedException();
        }
        public bool     ReceiveToken()
        {
            throw new NotImplementedException();
        }

        public bool DebugLock()
        {
            _accessingCriticalRegion = true;
            return true;
        }
        public bool DebugUnlock()
        {
            _accessingCriticalRegion = false;
            return true;
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
        private ChannelFactory<ICalendarService>    CreateChannelFactory(string ipAndPort)
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
