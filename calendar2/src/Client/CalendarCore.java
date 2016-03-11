public class CalendarCore 
{
    public enum CalendarState
    {
        Disconnected,
        Connecting,
        Connected
    }
    
    public  CalendarState           State;
    private CalendarNetworkManager  _networkManager;
    
    public CalendarCore()
    {
        State = CalendarState.Disconnected;
            
        _networkManager = new CalendarNetworkManager();
    }
    
    public String GetIPString()
    {
        return _networkManager.IP();
    }
    
    public void Exit()
    {
        System.exit(0);
    }
    
    public boolean CreateCalendarEvent(CalendarEvent calendarEvent)
    {
        return _networkManager.CreateCalendarEvent(calendarEvent);
    }
    public boolean EditCalendarEvent(CalendarEvent calendarEvent)
    {
        return _networkManager.EditCalendarEvent(calendarEvent);
    }
    public boolean DeleteCalendarEvent(CalendarEvent calendarEvent)
    {
        return _networkManager.DeleteCalendarEvent(calendarEvent);
    }
    public CalendarEvent[] GetCalendarEvents()
    {
        return _networkManager.GetCalendarEvents();
    }
    
    public boolean JoinNetwork(String targetIPAddress)
    {
        if(State == CalendarState.Disconnected)
        {
            State = CalendarState.Connecting;
        
            boolean result = _networkManager.JoinNetwork(targetIPAddress);
        
            State = result ? CalendarState.Connected : CalendarState.Disconnected;
    
            return result;
        }
        return false;
    }
    public boolean LeaveNetwork()
    {
        if(State != CalendarState.Disconnected)
        {
            boolean result = _networkManager.LeaveNetwork();
            State = CalendarState.Disconnected;
            return result;
        }
        return false;
    }
    
    public CalendarUser[] GetCalendarUsers()
    {
        return _networkManager.GetCalendarUsers();
    }
    public boolean ClearCalendarEvents()
    {
        return _networkManager.ClearCalendarEvents();
    }
    public boolean ClearUsers()
    {
        return _networkManager.ClearUsers();
    }
    
    public void Lock()
    {
        _networkManager.Lock();
    }
    public void Unlock()
    {
        _networkManager.Unlock();
    }
}
