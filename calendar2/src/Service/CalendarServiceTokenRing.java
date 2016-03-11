
import java.net.URL;
import java.util.ArrayList;
import org.apache.xmlrpc.client.XmlRpcClient;
import org.apache.xmlrpc.client.XmlRpcClientConfigImpl;

public class CalendarServiceTokenRing implements ICalendarService {

    private CalendarDatabaseManager _calendarDatabaseManager = new CalendarDatabaseManager();
    private static Thread _tokenThread;

    private static volatile boolean _requestingCriticalRegion = false;
    private static volatile boolean _accessingCriticalRegion = false;
    private static volatile boolean _hasToken = false;

    public boolean CreateCalendarEvent(String calendarEventString) {
        _requestingCriticalRegion = true;

        // If we are connected and we don't have token, wait.
        if (DecodeUsers(GetUsers()).length > 1) {
            while (!_hasToken) {

            }
        }

        // Start accessing critical region.
        _accessingCriticalRegion = true;

        boolean result = true;

        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
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
        _requestingCriticalRegion = true;

        if (DecodeUsers(GetUsers()).length > 1) {
            while (!_hasToken) {

            }
        }
        // Start accessing critical region.
        _accessingCriticalRegion = true;

        boolean result = true;

        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
            XmlRpcClient remoteServiceChannel = CreateServiceChannel(user.IPAddress);
            try {
                Object[] params = new Object[]{calendarEventString};
                boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.EditCalendarEventInternal", params);
                if (!newResult) {
                    System.out.println("Edit failed. Attempting add.");
                    newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.AddCalendarEventInternal", params);
                    if (!newResult) {
                        result = false;
                    }
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
        _requestingCriticalRegion = true;
        if (DecodeUsers(GetUsers()).length > 1) {
            while (!_hasToken) {

            }
        }
        // Start accessing critical region.
        _accessingCriticalRegion = true;

        boolean result = true;

        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
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

        boolean localExist = false;
        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
            if (user.IPAddress == Server.IPAndPort()) {
                localExist = true;
            }
        }
        if (!localExist && users.length == 0) {
            RegisterUser(Server.IPAndPort());
        }

        return true;
    }

    public static void TokenCoroutine() {
        while (true) {
            if (_hasToken) {
                while (_requestingCriticalRegion || _accessingCriticalRegion) {
                    System.out.println("Passing Thread: Using token!");
                }

                System.out.println("Passing thread: Token free! Passing.");

                XmlRpcClient remoteServiceChannel = CreateServiceChannel(Server.IPAndPort());
                try {
                    // Setup a channel for remote service.
                    _hasToken = false;
                    Object[] params = new Object[]{};
                    boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.PassToken", params);

                } catch (Exception e) {
                    System.out.println("Passing thread: Token passing failed! Trying next user!");
                }

                System.out.println("Passing thread: Token passing complete.");
            }
        }
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
            usersString += "---" + Server.IPAndPort();

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

                if (user.IPAddress.equals(Server.IPAndPort()))
                        continue;
                
                remoteServiceChannel = CreateServiceChannel(user.IPAddress);

                params = new Object[]{Server.IPAndPort()};
                boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.RegisterUser", params);

                if (!newResult) {
                    result = false;
                }
            }

            // If network consists of me and someone else (if network was just initiated):
            if (users.length == 2) /* make this two */ {
                ReceiveToken();
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

    public String RequestLock(String timestampString) {
        return "";
    }

    public boolean Initialize() {   
        System.out.println("Initializing.");
        
        //if (_tokenThread == null || !_tokenThread.isAlive()) {
            _tokenThread = new Thread(new TokenThread());
            _tokenThread.start();
        //}

        boolean localExist = false;
        CalendarUser[] users = DecodeUsers(GetUsers());
        for (CalendarUser user : users) {
            if (user.IPAddress == Server.IPAndPort()) {
                localExist = true;
            }
        }
        if (!localExist && users.length == 0) {
            RegisterUser(Server.IPAndPort());
        }

        System.out.println("Initialization complete.");

        return true;
    }

    public boolean PassToken() {
        boolean result = false;
 
        // We are done with token. Attempt passing it.
        _hasToken = false;

        int ownIndex = -1;
        int tarIndex;
        
        // Get index of the next user.
        CalendarUser[] users = DecodeUsers(GetUsers());
        for(int i = 0; i < users.length; i++)
        {
            if(users[i].IPAddress.equals(Server.IPAndPort()))
            {
                System.out.println("Found own index: " + ownIndex);
                ownIndex = i;
                break;
            }
        }
        tarIndex = ownIndex + 1 == users.length ? 0 : ownIndex + 1;

        System.out.println("Initial target index is: " + tarIndex);

        // Pass it to the next available user.
        while (!result) 
        {
            try 
            {
                    System.out.println("Passing token to: " + users[tarIndex].IPAddress);

                    XmlRpcClient remoteServiceChannel = CreateServiceChannel(users[tarIndex].IPAddress);

                    Object[] params = new Object[]{};
                    boolean newResult = (boolean) remoteServiceChannel.execute("BitCalendarService.ReceiveToken", params);
                    
                    System.out.println("Passed token to: " + users[tarIndex].IPAddress);
                    
                    result = true;
            }
            catch(Exception e)
            {
                System.out.println("Pass exception!");
                result   = false;
                tarIndex = tarIndex + 1 == users.length ? 0 : tarIndex + 1;
            }
        }
        return result;
    }

    public boolean ReceiveToken() {
        System.out.println("Received token!");

        _hasToken = true;

        return true;
    }

    public boolean DebugLock() {
        _requestingCriticalRegion = true;

        return true;
    }

    public boolean DebugUnlock() {
        _requestingCriticalRegion = false;

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

    private static XmlRpcClient CreateServiceChannel(String ipAndPort) {
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
