using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BitCalendar.Core.Network;
using BitCalendarService;

namespace BitCalendar.Core
{
    public class CalendarCore
    {
        public enum CalendarState
        {
            Disconnected,
            Connecting,
            Connected
        }
        
        private CalendarState           _state;
        public  CalendarState           State
        {
            get { return _state; }
            private set 
            {
                _state = value;
                if (OnCalendarStateChanged != null)
                    OnCalendarStateChanged(_state);
            }
        }

        public  List<CalendarUser>      CalendarUsers       { get; private set; }
        public  List<CalendarEvent>     CalendarEvents      { get; private set; }
        private CalendarNetworkManager  _networkManager;

        private bool                    _autoUpdate = false;
        private Thread                  _updateThread;

        public delegate void                        CalendarStateChangeHandler(CalendarState state);
        public delegate void                        CalendarUpdateHandler();

        public event    CalendarUpdateHandler       OnCalendarUpdated;
        public event    CalendarStateChangeHandler  OnCalendarStateChanged;
        public event    CalendarUpdateHandler       OnCalendarUsersUpdated;

        public CalendarCore()
        {
            State = CalendarState.Disconnected;
            
            _networkManager = new CalendarNetworkManager();

            if (_autoUpdate)
            {
                UpdateCalendarEvents();

                _updateThread = new Thread(UpdateCoroutine);
                _updateThread.Start();
            }
        }
        ~CalendarCore()
        {
            LeaveNetwork();
        }

        public void UpdateCoroutine()
        {
            while (true)
            {
                var calendarEvents = _networkManager.GetCalendarEvents().ToList();

                if (CalendarEvents != null && CalendarEvents.Count == calendarEvents.Count)
                {
                    if (CalendarEvents.Where((t, i) =>
                            calendarEvents[i].UniqueID == t.UniqueID &&
                            calendarEvents[i].StartDate == t.StartDate &&
                            calendarEvents[i].EndDate == t.EndDate &&
                            calendarEvents[i].CreatorID == t.CreatorID &&
                            calendarEvents[i].Header == t.Header &&
                            calendarEvents[i].Description == t.Description).Any())
                        return;

                    Console.Write("Update necessary. Updating.");
                }

                UpdateUsers();

                Thread.Sleep(1000);
            }
        }

        public bool CreateCalendarEvent(CalendarEvent calendarEvent)
        {
            try
            {
                var result = _networkManager.CreateCalendarEvent(calendarEvent);
                if(_autoUpdate)
                    UpdateCalendarEvents();
                return result;
            }
            catch (Exception)
            {
                return false;
            }     
        }
        public bool EditCalendarEvent(CalendarEvent calendarEvent)
        {
            try
            {
                var result = _networkManager.EditCalendarEvent(calendarEvent);
                if (_autoUpdate)
                    UpdateCalendarEvents();
                return result;
            }
            catch (Exception)
            {
                return false;
            }  
        }
        public bool DeleteCalendarEvent(CalendarEvent calendarEvent)
        {
            try
            {
                var result = _networkManager.DeleteCalendarEvent(calendarEvent);
                if (_autoUpdate)
                    UpdateCalendarEvents();
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void UpdateUsers()
        {
            CalendarUsers = _networkManager.GetUsers().ToList();
            if (CalendarUsers.Any())
                State = CalendarState.Connected;
            if (OnCalendarUsersUpdated != null)
                OnCalendarUsersUpdated();
        }
        public void UpdateCalendarEvents()
        {
            CalendarEvents = _networkManager.GetCalendarEvents().ToList();
            if (OnCalendarUpdated != null)
                OnCalendarUpdated();
        }
        public bool ClearCalendarEvents()
        {
            try
            {
                return _networkManager.ClearCalendarEventsTable();
            }
            catch (Exception)
            {
                return false;
            }        
        }
        public bool ClearUsers()
        {
            try
            {
                return _networkManager.ClearUsersTable();
            }
            catch (Exception)
            {
                return false;
            }      
        }

        public string IP()
        {
            return _networkManager.IP;
        }
        public bool JoinNetwork(string targetIPAddress)
        {
            State = CalendarState.Connecting;

            var result = _networkManager.JoinNetwork(targetIPAddress);

            //if (!result)
            //    _networkManager.LeaveNetwork();

            State = result ? CalendarState.Connected : CalendarState.Disconnected;
            
            if (_autoUpdate)
                UpdateCalendarEvents();

            return result;
        }
        public bool LeaveNetwork()
        {
            var result = false;

            if (State == CalendarState.Connected)
            {
                result = _networkManager.LeaveNetwork();
                State = CalendarState.Disconnected;

                if (_autoUpdate)
                    UpdateCalendarEvents();
            }      

            return result;
        }

        public void Lock()
        {
            _networkManager.Lock();
        }
        public void Unlock()
        {
            _networkManager.Unlock();
        }

        public void Abort()
        {
            if(_updateThread != null)
                _updateThread.Abort();
        }

        public List<CalendarUser> UpdateAndGetUsers()
        {
            try
            {
                CalendarUsers = _networkManager.GetUsers().ToList();
                return CalendarUsers;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<CalendarEvent> UpdateAndGetEvents()
        {
            try
            {
                CalendarEvents = _networkManager.GetCalendarEvents().ToList();
                return CalendarEvents;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
