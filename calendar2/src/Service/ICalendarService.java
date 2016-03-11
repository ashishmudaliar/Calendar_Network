public interface ICalendarService
{
    public boolean  CreateCalendarEvent(String calendarEventString);
    public boolean  EditCalendarEvent(String calendarEventString);
    public boolean  DeleteCalendarEvent(String calendarEventString);
    public String   GetCalendarEvents();
    public String   GetUsers();
    
    public boolean  Connect(String targetIPAddress);
    public boolean  Disconnect();
    public boolean  RegisterUser(String calendarUserString);
    public boolean  DeregisterUser(String calendarUserString);
   
    public boolean  ClearCalendarEventsTable();
    public boolean  ClearUsersTable();
    
    public boolean  CreateCalendarEventInternal(String calendarEventString);
    public boolean  EditCalendarEventInternal(String calendarEventString);
    public boolean  DeleteCalendarEventInternal(String calendarEventString);
        
    public boolean  Initialize();
    public String   RequestLock(String timestampString);
    public boolean  PassToken();
    public boolean  ReceiveToken();
    public boolean  DebugLock();
    public boolean  DebugUnlock();  
}
