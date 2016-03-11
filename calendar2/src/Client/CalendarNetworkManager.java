import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.MalformedURLException;
import java.net.NetworkInterface;
import java.net.URL;
import java.util.ArrayList;
import java.util.Enumeration;
import org.apache.xmlrpc.client.XmlRpcClient;
import org.apache.xmlrpc.client.XmlRpcClientConfigImpl;


public class CalendarNetworkManager 
{
    static int Port = 8080;
    static String _baseAddress = "/BitCalendarService";
    
    private XmlRpcClient    _localService;

    public CalendarNetworkManager()
    {
        _localService = CreateServiceChannel(IP() + ":" + Port);
        try
        {
            Object[] params = new Object[]{};
            boolean result = (boolean)_localService.execute("BitCalendarService.Initialize",params);
            System.out.println("Service initialized!");
        }
        catch(Exception e)
        {
            System.out.println("Service initialization error!");
            e.printStackTrace();
        }
    }
    
    public String IP() 
    {
        String ip = null;
        int flag = 0;
        try 
        {
            Enumeration<NetworkInterface> interfaces = NetworkInterface.getNetworkInterfaces();
            while (interfaces.hasMoreElements() && flag == 0)
            {
                NetworkInterface iface = interfaces.nextElement();
                
                // Filters out 127.0.0.1 and inactive interfaces
                if (iface.isLoopback() || !iface.isUp())
                    continue;

                Enumeration<InetAddress> addresses = iface.getInetAddresses();
                while (addresses.hasMoreElements()) {
                    InetAddress addr = addresses.nextElement();
                    if (addr instanceof Inet4Address) {
                        ip = addr.getHostAddress();
                        flag = 1;
                    }
                }
            }
            return ip;
        } 
        catch (Exception e) 
        {
            return "Error retrieving IP address.";
        }
    }
    // Use this function to connect.
    public boolean JoinNetwork(String targetIPAddress)
    {
        try
        {
            Object[] params = new Object[]{targetIPAddress};
            boolean result = (boolean)_localService.execute("BitCalendarService.Connect",params);
            return result;
        }
        catch(Exception e)
        {
            return false;
        }
    }
    // Use this function to disconnect.
    public boolean LeaveNetwork() 
    {
        try
        {
            Object[] params = new Object[]{};
            boolean result = (boolean)_localService.execute("BitCalendarService.Disconnect",params);
            return result;
        }
        catch(Exception e)
        {
            return false;
        }
    }
    
    // Local & remote.
    public boolean CreateCalendarEvent(CalendarEvent calendarEvent)
    {
        try
        {
            Object[] params = new Object[]{calendarEvent.EncodeToString()};
            boolean result = (boolean)_localService.execute("BitCalendarService.CreateCalendarEvent",params);
            return result;
        }
        catch(Exception e)
        {
            return false;
        }
    }
    public boolean EditCalendarEvent(CalendarEvent calendarEvent)
    {
        try
        {
            Object[] params = new Object[]{calendarEvent.EncodeToString()};
            boolean result = (boolean)_localService.execute("BitCalendarService.EditCalendarEvent",params);
            return result;
        }
        catch(Exception e)
        {
            return false;
        }
    }
    public boolean DeleteCalendarEvent(CalendarEvent calendarEvent)
    {
        try
        {
            Object[] params = new Object[]{calendarEvent.EncodeToString()};
            boolean result = (boolean)_localService.execute("BitCalendarService.DeleteCalendarEvent",params);
            return result;
        }
        catch(Exception e)
        {
            return false;
        }
    }
    
    // Local-only functions.
    public CalendarEvent[] GetCalendarEvents()
    {
        try
        {
            Object[] params = new Object[]{};
            String result = (String)_localService.execute("BitCalendarService.GetCalendarEvents",params);
            return DecodeCalendarEvents(result); 
        }
        catch(Exception e)
        {
            return new CalendarEvent[0];
        } 
    }
    public CalendarUser[] GetCalendarUsers()
    {
        try
        {
            Object[] params = new Object[]{};
            String result = (String)_localService.execute("BitCalendarService.GetUsers",params);
            return DecodeUsers(result); 
        }
        catch(Exception e)
        {
            return new CalendarUser[0];
        }
    }
    public boolean ClearCalendarEvents()
    {  
        try
        {
            Object[] params = new Object[]{};
            boolean result = (boolean)_localService.execute("BitCalendarService.ClearCalendarEventsTable",params);
            return result;
        }
        catch(Exception e)
        {
            return false;
        } 
    }
    public boolean ClearUsers()
    {
        try
        {
            Object[] params = new Object[]{};
            boolean result = (boolean)_localService.execute("BitCalendarService.ClearUsersTable",params);
            return result;
        }
        catch(Exception e)
        {
            return false;
        }   
    }
  
    public void Lock()
    {
        try
        {
            Object[] params = new Object[]{};
            boolean result = (boolean)_localService.execute("BitCalendarService.DebugLock",params);
        }
        catch(Exception e)
        {
        }   
    }
    public void Unlock()
    {
        try
        {
            Object[] params = new Object[]{};
            boolean result = (boolean)_localService.execute("BitCalendarService.DebugUnlock",params);
        }
        catch(Exception e)
        {
        }  
    }
    
    private CalendarUser[] DecodeUsers(String calendarUsersString) 
    {
        String[] splitArray = calendarUsersString.split("\\-\\-\\-");
        
        ArrayList<CalendarUser> calendarUsers = new ArrayList<>();
        for(int i = 0; i < splitArray.length; i++)
        {
            CalendarUser newUser = new CalendarUser();
            if(newUser.DecodeFromString(splitArray[i]))
            {
                calendarUsers.add(newUser);
            }
        }
        
        return calendarUsers.toArray(new CalendarUser[0]);
    }
    private CalendarEvent[] DecodeCalendarEvents(String calendarEventsString) 
    {
        if (calendarEventsString == null || calendarEventsString.trim().equals(""))
            return new CalendarEvent[0];
        
        String[] splitArray = calendarEventsString.split("\\-\\-\\-");
        
        ArrayList<CalendarEvent> calendarEvents = new ArrayList<>();
        for(int i = 0; i < splitArray.length; i++)
        {
            CalendarEvent newEvent = new CalendarEvent();
            if(newEvent.DecodeFromString(splitArray[i]))
                calendarEvents.add(newEvent);
        }
        
        return calendarEvents.toArray(new CalendarEvent[0]);
    }
    
    private XmlRpcClient CreateServiceChannel(String ipAndPort)
    {
        System.out.println("Creating service channel to: http://" + ipAndPort + _baseAddress);
        XmlRpcClientConfigImpl config = new XmlRpcClientConfigImpl();
        try
        {
            config.setServerURL(new URL("http://" + ipAndPort + _baseAddress));
        }
        catch (MalformedURLException e) 
        {
            e.printStackTrace();
        }
        
        XmlRpcClient client = new XmlRpcClient();
	client.setConfig(config);
        
        return client;
    }
    
}
