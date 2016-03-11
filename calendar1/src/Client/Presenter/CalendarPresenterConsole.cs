using System;
using System.Collections.Generic;
using System.Globalization;
using BitCalendar.Core;
using BitCalendar.Core.Network;
using BitCalendar.View;
using BitCalendarService;

namespace BitCalendar.Presenter
{
    class CalendarPresenterConsole
    {
        private CalendarViewConsole _view;
        private CalendarCore        _core;

        public CalendarPresenterConsole(CalendarCore calendarCore, CalendarViewConsole calendarView)
        {
            _core = calendarCore;
            _view = calendarView;
            _view.Presenter = this;

            _view.PrintLine("Welcome to B-IT Calendar!");
            _view.PrintLine("Type 'help' to see the available commands.");
        }

        public void ParseInput(String input)
        {
            var inputList = input.Split(new[] {" "}, StringSplitOptions.None);
            for(int i = 0; i < inputList.Length; i++)
                inputList[i] = inputList[i].Trim();

            if (inputList.Length == 0)
                return;

            switch (inputList[0].ToLower())
            {
                case "help":
                    ShowHelp(inputList);
                    break;
                case "ip":
                    ShowIP();
                    break;

                case "list":
                    ShowEvents();
                    break;
                case "add":
                    AddEvent(inputList);
                    break;
                case "edit":
                    EditEvent(inputList);
                    break;
                case "delete":
                    DeleteEvent(inputList);
                    break;

                case "state":
                    ShowState();
                    break;
                case "connect":
                    Connect(inputList);
                    break;
                case "disconnect":
                    Disconnect(inputList);
                    break;

                case "exit":
                    Exit();
                    break;

                    // Secret functions.
                case "test":
                    Test();
                    break;
                case "listusers":
                    ShowUsers();
                    break;
                case "wipeevents":
                    WipeCalendarEvents();
                    break;
                case "wipeusers":
                    WipeCalendarUsers();
                    break;
                case "lock":
                    Lock();
                    break;
                case "unlock":
                    Unlock();
                    break;
                default:
                    _view.PrintLine("Undefined command. Type 'help' for available commands.");
                    break;
            }
        }
        public bool CheckDateArgumentValidity(String dateArgument)
        {
            try
            {
                var temp = DateTime.Parse(dateArgument, null, DateTimeStyles.RoundtripKind);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ShowHelp(String[] args)
        {
            _view.PrintLine("#######################");
            if (args != null && args.Length == 2)
            {
                switch (args[1])
                {
                    case "help":
                        ShowHelp(null);
                        break;
                    case "ip":
                        _view.PrintLine("Retrieves your current IP.");
                        _view.PrintLine("Usage: ip");
                        break;
                    case "list":
                        _view.PrintLine("Displays all events in calendar.");
                        _view.PrintLine("Usage: list");
                        break;
                    case "add":
                        _view.PrintLine("Adds a new event to the calendar.");
                        _view.PrintLine("Usage: add [Creator ID] [Start Date] [End Date] [Header] [Description]");
                        _view.PrintLine("Note that creator ID, start date, end date and header are one word long.");
                        _view.PrintLine("Description can be any number of words.");
                        _view.PrintLine("StartDate and EndDate must be in following format: yyyy-MM-ddTHH:mm:ssZ.");
                        break;
                    case "edit":
                        _view.PrintLine("Edits an existing event in the calendar.");
                        _view.PrintLine("Usage: edit [UniqueID] [CreatorID] [StartDate] [EndDate] [Header] [Description]");
                        _view.PrintLine("You can learn the unique ID of the event you want to edit using the list command.");
                        _view.PrintLine("Note that CreatorID, StartDate, EndDate and Header are one word long.");
                        _view.PrintLine("Description can be any number of words.");
                        _view.PrintLine("StartDate and EndDate must be in following format: yyyy-MM-ddTHH:mm:ssZ.");
                        break;
                    case "delete":
                        _view.PrintLine("Deletes the event with the given unique ID.");
                        _view.PrintLine("Usage: delete [Unique ID]");
                        break;

                    case "state":
                        _view.PrintLine("Retrieves current connection state.");
                        _view.PrintLine("Usage: state");
                        break;
                    case "connect":
                        _view.PrintLine("Attempts to connect to the network with the given IP address.");
                        _view.PrintLine("Usage: connect [IP Address]:[Port]");
                        break;
                    case "disconnect":
                        _view.PrintLine("Attempts to disconnect from the current network (if any).");
                        _view.PrintLine("Usage: disconnect");
                        break;

                    case "exit":
                        _view.PrintLine("Exits the application.");
                        _view.PrintLine("Usage: exit");
                        break;

                    default:
                        ShowHelp(null);
                        break;
                }
            }
            else
            {
                _view.PrintLine(
                "Available commands: \n" +
                "   Help        : Shows the help string.\n" +
                "   IP          : Shows your IP.\n" +
                "   List        : Lists all calendar events.\n" +
                "   Add         : Adds a new calendar event.\n" +
                "   Edit        : Edits a calendar event.\n" +
                "   Delete      : Deletes a calendar event.\n" +
                "   State       : Shows the current connection state.\n" +
                "   Connect     : Connects to the given network.\n" +
                "   Disconnect  : Disconnects from the network.\n" +
                "   Exit        : Exits the system.\n" +
                "Type help [command name] for the usage of each command.");
            }
            _view.PrintLine("#######################");
        }
        public void ShowIP()
        {
            _view.PrintLine("Your IP is: " + _core.IP() + ":" + CalendarNetworkManager.Port + ".");
        }

        public void ShowEvents()
        {
            _view.PrintLine("Listing calendar events.");
            _view.PrintLine("########################");

            var calendarEvents = _core.UpdateAndGetEvents();
            if (calendarEvents == null)
            {
                _view.PrintLine("No events!");
            }
            else
            {
                _view.PrintLine("Event Count: " + calendarEvents.Count);
                foreach (var calendarEvent in calendarEvents)
                {
                    _view.PrintLine("------------------------");
                    _view.PrintCalendarEvent(calendarEvent);
                }
            }

            _view.PrintLine("------------------------");
            _view.PrintLine("########################");
            _view.PrintLine("Listed calendar events.");
        }
        public void AddEvent(String[] args)
        {
            if (args.Length < 6)
                _view.PrintLine("Invalid arguments. Type 'help add' for usage.");
            else
            {
                CalendarEvent newEvent = new CalendarEvent();

                // Set creator ID.
                newEvent.CreatorID = args[1];

                // Check and set start date.
                if (CheckDateArgumentValidity(args[2]))
                    newEvent.StartDate = args[2];
                else
                {
                    _view.PrintLine("Invalid arguments. "
                            + "Start date is not in format: yyyy-MM-ddTHH:mm:ssZ.");
                    return;
                }

                // Check and set end date.
                if (CheckDateArgumentValidity(args[3]))
                    newEvent.EndDate = args[3];
                else
                {
                    _view.PrintLine("Invalid arguments. "
                            + "End date is not in format: yyyy-MM-ddTHH:mm:ssZ.");
                    return;
                }

                // Set header.
                newEvent.Header = args[4];

                // Set description with rest.
                String descriptionString = "";
                for (int i = 5; i < args.Length; i++)
                    descriptionString += args[i] + " ";
                descriptionString = descriptionString.Substring(0, descriptionString.Length - 1);
                newEvent.Description = descriptionString;

                _view.PrintLine("Attempting to add event: " + newEvent.Header + ".");
                if (_core.CreateCalendarEvent(newEvent))
                    _view.PrintLine("Successfully added event: " + newEvent.Header + ".");
                else
                    _view.PrintLine("Failed to add event: " + newEvent.Header + ".");
            }
        }
        public void EditEvent(String[] args)
        {
            if (args.Length < 7)
                _view.PrintLine("Invalid arguments. Type 'help edit' for usage.");
            else
            {
                CalendarEvent editedEvent = new CalendarEvent();

                // Check and set unique ID.
                try
                {
                    editedEvent.UniqueID = int.Parse(args[1]);
                }
                catch (Exception e)
                {
                    _view.PrintLine("Invalid arguments. Non-numeric unique ID: "
                            + args[1] + ".");
                    return;
                }

                // Set creator ID.
                editedEvent.CreatorID = args[2];

                // Check and set start date.
                if (CheckDateArgumentValidity(args[3]))
                    editedEvent.StartDate = args[3];
                else
                {
                    _view.PrintLine("Invalid arguments. "
                            + "Start date is not in format: yyyy-MM-ddTHH:mm:ssZ.");
                    return;
                }

                // Check and set end date.
                if (CheckDateArgumentValidity(args[4]))
                    editedEvent.EndDate = args[4];
                else
                {
                    _view.PrintLine("Invalid arguments. "
                            + "End date is not in format: yyyy-MM-ddTHH:mm:ssZ.");
                    return;
                }

                // Set header.
                editedEvent.Header = args[5];

                // Set description with rest.
                String descriptionString = "";
                for (int i = 6; i < args.Length; i++)
                    descriptionString += args[i] + " ";
                descriptionString = descriptionString.
                        Substring(0, descriptionString.Length - 1);
                editedEvent.Description = descriptionString;

                _view.PrintLine("Attempting to edit event: " + editedEvent.Header + ".");
                if (_core.EditCalendarEvent(editedEvent))
                    _view.PrintLine("Successfully editted event: " + editedEvent.Header + ".");
                else
                    _view.PrintLine("Failed to edit event: " + editedEvent.Header + ".");
            }
        }
        public void DeleteEvent(String[] args)
        {
            if (args.Length != 2)
                _view.PrintLine("Invalid arguments. Use the following format: delete [Unique ID].");
            else
            {
                int convertedValue = -1;
                try
                {
                    convertedValue = int.Parse(args[1]);
                }
                catch (Exception e)
                {
                    _view.PrintLine("Invalid arguments. Non-numeric unique ID: "
                            + args[1] + ".");
                    return;
                }

                _view.PrintLine("Attempting to delete event: " + args[1] + ".");
                var calendarEvent = new CalendarEvent(convertedValue, "NA", "NA", "NA", "NA", "NA");
                if (_core.DeleteCalendarEvent(calendarEvent))
                    _view.PrintLine("Successfully deleted event: " + args[1] + ".");
                else
                    _view.PrintLine("Failed to delete event: " + args[1] + ".");
            }
        }

        public void ShowState()
        {
            _view.PrintLine("Connection state is: " + _core.State);
        }
        public void Connect(String[] args)
        {
            if (args.Length!= 2)
                _view.PrintLine("Invalid arguments. Use the following format: connect [IP Address].");
            else
            {
                _view.PrintLine("Attempting to join the network: " + args[1] + ".");
                if (_core.JoinNetwork(args[1]))
                    _view.PrintLine("Successfully joined the network: " + args[1] + ".");
                else
                    _view.PrintLine("Failed to join the network: " + args[1] + ".");
            }
        }
        public void Disconnect(String[] args)
        {
            _view.PrintLine("Attempting to leave the network.");
            if (_core.State != CalendarCore.CalendarState.Connected)
                _view.PrintLine("Failed to leave the network: You are not connected to any.");
            else if (_core.LeaveNetwork())
                _view.PrintLine("Successfully left the network.");
            else
                _view.PrintLine("Failed to leave the network: Refused.");
        }

        public void Exit()
        {
            _view.PrintLine("Exiting B-IT calendar.");
            _core.Abort();
            Environment.Exit(0);
        }

        public void Test()
        {
            var args = new List<String>();
            args.Add("add");
            args.Add("TestUser");
            args.Add("2013-12-11T12:30:00Z");
            args.Add("2013-12-11T14:30:00Z");
            args.Add("TestEvent");
            args.Add("Test Event Description");
            AddEvent(args.ToArray());
        }

        public void ShowUsers()
        {
            _view.PrintLine("Listing calendar users.");
            _view.PrintLine("#######################");

            var calendarUsers = _core.UpdateAndGetUsers();
            if (calendarUsers == null)
            {
                _view.PrintLine("No users connected!");
            }
            else
            {
                _view.PrintLine("Connected User Count: " + calendarUsers.Count);
                foreach (var calendarUser in calendarUsers)
                    _view.PrintLine(calendarUser.IPAddress);
            }
            
            _view.PrintLine("#######################");
            _view.PrintLine("Listed calendar users.");
        }
        public void WipeCalendarEvents()
        {
            _view.PrintLine("Attempting to wipe the calendar events.");
            if (_core.ClearCalendarEvents())
                _view.PrintLine("Successfully wiped the calendar events.");
            else
                _view.PrintLine("Failed to wipe the calendar events.");
        }
        public void WipeCalendarUsers()
        {
            _view.PrintLine("Attempting to wipe the calendar users.");
            if (_core.ClearUsers())
                _view.PrintLine("Successfully wiped the calendar users.");
            else
                _view.PrintLine("Failed to wipe the calendar users.");

        }
        public void Lock()
        {
            _core.Lock();
        }
        public void Unlock()
        {
            _core.Unlock();
        }
    }
}
