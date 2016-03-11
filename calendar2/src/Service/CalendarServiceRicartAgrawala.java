
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import org.apache.xmlrpc.client.XmlRpcClient;
import org.apache.xmlrpc.client.XmlRpcClientConfigImpl;

public class CalendarServiceRicartAgrawala implements ICalendarService {

    private CalendarDatabaseManager _calendarDatabaseManager;
    private static boolean _requestingCriticalRegion = false;
    private static boolean _accessingCriticalRegion = false;
    private static int _criticalRegionNo = -1;

    private static LamportClock _lamportClock = new LamportClock();

    public boolean CreateCalendarEvent(String calendarEventString) {
        _criticalRegionNo = -1;
        _requestingCriticalRegion = true;

        // Request lock.
        QueryLock();

        // Start accessing critical region.
        _accessingCriticalRegion = true;

        _lamportClock.Increment();

        boolean result = CreateCalendarEventInternal(calendarEventString);

        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
            _lamportClock.Increment();

            XmlRpcClient remoteServiceChannel = CreateServiceChannel(user.IPAddress);
            try {
                Object[] params = new Object[]{calendarEventString};
                boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.CreateCalendarEventInternal", params);
                if (!newResult) {
                    result = false;
                }
            } catch (Exception e) {
                System.out.println("Remote create error!");
            }

        }

        _requestingCriticalRegion = false;
        _accessingCriticalRegion = false;

        return result;
    }

    public boolean EditCalendarEvent(String calendarEventString) {
        _criticalRegionNo = -1;
        _requestingCriticalRegion = true;

        // Request lock.
        QueryLock();

        // Start accessing critical region.
        _accessingCriticalRegion = true;

        _lamportClock.Increment();

        boolean result = EditCalendarEventInternal(calendarEventString);

        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
            _lamportClock.Increment();

            XmlRpcClient remoteServiceChannel = CreateServiceChannel(user.IPAddress);
            try {
                Object[] params = new Object[]{calendarEventString};
                boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.EditCalendarEventInternal", params);
                if (!newResult) {
                    result = false;
                }
            } catch (Exception e) {
                System.out.println("Remote edit error!");
            }

        }

        _requestingCriticalRegion = false;
        _accessingCriticalRegion = false;

        return result;
    }

    public boolean DeleteCalendarEvent(String calendarEventString) {
        _criticalRegionNo = -1;
        _requestingCriticalRegion = true;

        // Request lock.
        QueryLock();

        // Start accessing critical region.
        _accessingCriticalRegion = true;

        _lamportClock.Increment();

        boolean result = DeleteCalendarEventInternal(calendarEventString);

        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
            _lamportClock.Increment();

            XmlRpcClient remoteServiceChannel = CreateServiceChannel(user.IPAddress);
            try {
                Object[] params = new Object[]{calendarEventString};
                boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.DeleteCalendarEventInternal", params);
                if (!newResult) {
                    result = false;
                }
            } catch (Exception e) {
                System.out.println("Remote delete error!");
            }

        }

        _requestingCriticalRegion = false;
        _accessingCriticalRegion = false;

        return result;
    }

    public String GetCalendarEvents() {
        String calendarEventsString = "";

        _calendarDatabaseManager = new CalendarDatabaseManager();

        CalendarEvent[] calendarEvents = _calendarDatabaseManager.GetCalendarEvents();
        String[] calendarEventStrings = new String[calendarEvents.length];
        for (int i = 0; i < calendarEvents.length; i++) {
            calendarEventStrings[i] = calendarEvents[i].EncodeToString();
            calendarEventsString += calendarEventStrings[i];
            if (i + 1 < calendarEvents.length) {
                calendarEventsString += "---";
            }
        }

        return calendarEventsString;
    }

    public boolean RegisterUser(String calendarUserString) {
        _calendarDatabaseManager = new CalendarDatabaseManager();
        return _calendarDatabaseManager.CreateUser(new CalendarUser(calendarUserString));
    }

    public boolean DeregisterUser(String calendarUserString) {
        _calendarDatabaseManager = new CalendarDatabaseManager();
        return _calendarDatabaseManager.DeleteUser(new CalendarUser(calendarUserString));
    }

    public String GetUsers() {
        String calendarUsersString = "";

        _calendarDatabaseManager = new CalendarDatabaseManager();

        CalendarUser[] calendarUsers = _calendarDatabaseManager.GetUsers();
        String[] calendarUserStrings = new String[calendarUsers.length];
        for (int i = 0; i < calendarUsers.length; i++) {
            calendarUserStrings[i] = calendarUsers[i].EncodeToString();
            calendarUsersString += calendarUserStrings[i];
            if (i + 1 < calendarUsers.length) {
                calendarUsersString += "---";
            }
        }

        return calendarUsersString;
    }

    public boolean SyncUsers(String calendarUsersString) {
        if (calendarUsersString == null) {
            return true;
        }

        String[] splitArray = calendarUsersString.split("\\-\\-\\-");

        ArrayList<CalendarUser> calendarUsers = new ArrayList<>();
        for (int i = 0; i < splitArray.length; i++) {
            CalendarUser newUser = new CalendarUser();
            if (newUser.DecodeFromString(splitArray[i])) {
                calendarUsers.add(newUser);
            }
        }

        _calendarDatabaseManager = new CalendarDatabaseManager();

        _calendarDatabaseManager.ClearUsers();
        _calendarDatabaseManager.CreateUsers(calendarUsers.toArray(new CalendarUser[0]));

        return true;
    }

    public boolean SyncCalendarEvents(String calendarEventsString) {
        if (calendarEventsString == null) {
            return true;
        }

        String[] splitArray = calendarEventsString.split("\\-\\-\\-");

        ArrayList<CalendarEvent> calendarEvents = new ArrayList<>();
        for (int i = 0; i < splitArray.length; i++) {
            CalendarEvent newEvent = new CalendarEvent();
            if (newEvent.DecodeFromString(splitArray[i])) {
                calendarEvents.add(newEvent);
            }
        }

        _calendarDatabaseManager = new CalendarDatabaseManager();

        _calendarDatabaseManager.ClearCalendarEvents();
        _calendarDatabaseManager.CreateCalendarEvents(calendarEvents.toArray(new CalendarEvent[0]));

        return true;
    }

    public boolean ClearCalendarEventsTable() {
        _calendarDatabaseManager = new CalendarDatabaseManager();
        _calendarDatabaseManager.ClearCalendarEvents();
        return true;
    }

    public boolean ClearUsersTable() {
        _calendarDatabaseManager = new CalendarDatabaseManager();
        _calendarDatabaseManager.ClearUsers();
        return true;
    }

    public boolean Connect(String targetIPAndPort) {
        boolean result;

        System.out.println("Attempting to connect to IP: " + targetIPAndPort);

        try {
            XmlRpcClient remoteServiceChannel = CreateServiceChannel(targetIPAndPort);
            Object[] params;

            System.out.println("Retrieving users list.");

            // Sync users from the target PC.
            params = new Object[]{};
            String usersString = (String) remoteServiceChannel.execute("BitCalendarService.GetUsers", params);
            usersString += "---" + targetIPAndPort;

            System.out.println("Retrieved users list.");
            System.out.println("Syncing users.");

            result = SyncUsers(usersString);

            System.out.println("Synced users.");
            System.out.println("Retrieving calendar events list.");

            params = new Object[]{};
            String calendarEventsString = (String) remoteServiceChannel.execute("BitCalendarService.GetCalendarEvents", params);

            System.out.println("Retrieved calendar events list.");
            System.out.println("Syncing calendar events.");

            if (result) {
                result = SyncCalendarEvents(calendarEventsString);
            }

            System.out.println("Synced calendar events.");

            // Register to other people's user table.
            CalendarUser[] users = DecodeUsers(GetUsers());
            for (CalendarUser user : users) {
                remoteServiceChannel = CreateServiceChannel(user.IPAddress);

                params = new Object[]{Server.IPAndPort()};
                boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.RegisterUser", params);

                if (!newResult) {
                    result = false;
                }
            }

        } catch (Exception e) {
            System.out.println("Exception connecting to network from IP: " + targetIPAndPort);
            return false;
        }

        return result;
    }

    public boolean Disconnect() {
        boolean result = true;

        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
            XmlRpcClient remoteServiceChannel = CreateServiceChannel(user.IPAddress);
            try {
                Object[] params = new Object[]{Server.IPAndPort()};
                boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.DeregisterUser", params);
                if (!newResult) {
                    result = false;
                }
            } catch (Exception e) {
                System.out.println("Deregister error!");
            }
        }

        ClearUsersTable();

        return result;
    }

    public boolean CreateCalendarEventInternal(String calendarEventString) {
        _calendarDatabaseManager = new CalendarDatabaseManager();
        return _calendarDatabaseManager.CreateCalendarEvent(new CalendarEvent(calendarEventString));
    }

    public boolean EditCalendarEventInternal(String calendarEventString) {
        _calendarDatabaseManager = new CalendarDatabaseManager();
        return _calendarDatabaseManager.EditCalendarEvent(new CalendarEvent(calendarEventString));

    }

    public boolean DeleteCalendarEventInternal(String calendarEventString) {
        _calendarDatabaseManager = new CalendarDatabaseManager();
        return _calendarDatabaseManager.DeleteCalendarEvent(new CalendarEvent(calendarEventString));

    }

    private void QueryLock() {
        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {

            System.out.println("Requesting create lock from: " + user.IPAddress);

            _lamportClock.Increment();

            ExtendedTimestamp extendedTimestamp = new ExtendedTimestamp(_lamportClock.CurrentTime, Server.IPAndPort(), _criticalRegionNo);

            XmlRpcClient remoteServiceChannel = CreateServiceChannel(user.IPAddress);
            try {
                Object[] params = new Object[]{extendedTimestamp.ToString()};
                String result = (String) remoteServiceChannel.execute("BitCalendarService.RequestLock", params);

                System.out.println("Response to create lock received from: " + user.IPAddress);

                ExtendedTimestamp othersTimestamp = ExtendedTimestamp.FromString(result);
                _lamportClock.Adjust(othersTimestamp.Time);

            } catch (Exception e) {
                System.out.println("Request lock error!");
            }
        }
        System.out.println("All lock responses receieved! Proceeding to critical region.");
    }

    public String RequestLock(String timestampString) {
        System.out.println("Received lock request. Timestamp string: " + timestampString);

        ExtendedTimestamp extTimestamp = ExtendedTimestamp.FromString(timestampString);

        if (_criticalRegionNo != extTimestamp.CriticalRegionNo) {

        } else if (!_accessingCriticalRegion && !_requestingCriticalRegion) {

        } else if (_accessingCriticalRegion) {
            // Queue loop.
            while (_accessingCriticalRegion) {
                System.out.println("Delaying response, accessing critical region.");
            }
        } else if (_requestingCriticalRegion) {
            if (_lamportClock.CurrentTime > extTimestamp.Time) {

            } else {
                // Queue loop.
                while (_requestingCriticalRegion) {
                    System.out.println("Delaying response, requesting critical region.");
                }
            }
        }

        System.out.println("Returning lock request!");
        _lamportClock.Adjust(extTimestamp.Time);
        return new ExtendedTimestamp(_lamportClock.CurrentTime, Server.IPAndPort(), extTimestamp.CriticalRegionNo).ToString();
    }

    public boolean Initialize() {
        return true;
    }

    public boolean PassToken() {
        return true;
    }

    public boolean ReceiveToken() {
        return true;
    }

    public boolean DebugLock() {
        _accessingCriticalRegion = true;
        return true;
    }

    public boolean DebugUnlock() {
        _accessingCriticalRegion = false;
        return true;
    }

    private CalendarUser[] DecodeUsers(String calendarUsersString) {
        String[] splitArray = calendarUsersString.split("\\-\\-\\-");

        ArrayList<CalendarUser> calendarUsers = new ArrayList<>();
        for (int i = 0; i < splitArray.length; i++) {
            CalendarUser newUser = new CalendarUser();
            if (newUser.DecodeFromString(splitArray[i])) {
                calendarUsers.add(newUser);
            }
        }

        return calendarUsers.toArray(new CalendarUser[0]);
    }

    private CalendarEvent[] DecodeCalendarEvents(String calendarEventsString) {
        if (calendarEventsString == null) {
            return new CalendarEvent[0];
        }

        String[] splitArray = calendarEventsString.split("\\-\\-\\-");

        ArrayList<CalendarEvent> calendarEvents = new ArrayList<>();
        for (int i = 0; i < splitArray.length; i++) {
            CalendarEvent newEvent = new CalendarEvent();
            if (newEvent.DecodeFromString(splitArray[i])) {
                calendarEvents.add(newEvent);
            }
        }

        return calendarEvents.toArray(new CalendarEvent[0]);
    }

    private XmlRpcClient CreateServiceChannel(String ipAndPort) {
        System.out.println("Creating service channel to: http://" + ipAndPort + Server.BaseAddress);
        XmlRpcClientConfigImpl config = new XmlRpcClientConfigImpl();
        try {
            config.setServerURL(new URL("http://" + ipAndPort + Server.BaseAddress));
        } catch (Exception e) {
            e.printStackTrace();
        }

        XmlRpcClient client = new XmlRpcClient();
        client.setConfig(config);

        return client;
    }
}
