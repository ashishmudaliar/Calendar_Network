import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.Statement;
import java.util.ArrayList;

public class CalendarDatabaseManager 
{   
    Connection connection = null;
    
    public void PreAction()
    {
        try 
        {
            Class.forName("org.sqlite.JDBC");
            connection = DriverManager.getConnection("jdbc:sqlite:BitCalendar.db");
  
            Statement createCalendarEventsStatement = connection.createStatement();
            String sql = "CREATE TABLE IF NOT EXISTS CalendarEvents" +
                   "(UniqueID       INTEGER     PRIMARY KEY     AUTOINCREMENT," +
                   " CreatorID      TEXT, " + 
                   " StartDate      TEXT, " + 
                   " EndDate        TEXT, " + 
                   " Header         TEXT, " + 
                   " Description    TEXT)"; 
            
            createCalendarEventsStatement.executeUpdate(sql);
            createCalendarEventsStatement.close();
            
            Statement createCalendarUsersStatement = connection.createStatement();
            sql = "CREATE TABLE IF NOT EXISTS CalendarUsers" +
                   "(IPAddress TEXT PRIMARY KEY NOT NULL)";

            createCalendarUsersStatement.executeUpdate(sql);
            createCalendarUsersStatement.close();
        } 
        catch (Exception e) 
        {
            System.err.println(e.getClass().getName() + ": " + e.getMessage());
        }
    }  
    public void PostAction()
    {
        try
        {
            connection.close();
        }
        catch (Exception e) 
        {
            System.err.println(e.getClass().getName() + ": " + e.getMessage());
        }        
    }
      
    public boolean CreateCalendarEvent(CalendarEvent calendarEvent)
    {
        System.out.println("Creating calendar event: " + calendarEvent.Header);
        
        boolean result = true;
        
        PreAction();
        
        try
        {
            Statement stmt = connection.createStatement();
            String sql = "INSERT INTO CalendarEvents" + 
                    "(CreatorID,StartDate,EndDate,Header,Description) " +
                    "VALUES (";
            
            if(calendarEvent.CreatorID != null)
                sql += "\"" + calendarEvent.CreatorID   + "\"";
            else
                sql += "\"\"";
            
            if(calendarEvent.StartDate != null)
                sql += ",\"" + calendarEvent.StartDate  + "\"";
            else
                sql += ",\"\"";
            
            if(calendarEvent.EndDate != null)
                sql += ",\"" + calendarEvent.EndDate    + "\"";
            else
                sql += ",\"\"";
            
            if(calendarEvent.Header != null)
                sql += ",\"" + calendarEvent.Header     + "\"";
            else
                sql += ",\"\"";
            
            if(calendarEvent.Description != null)
                sql += ",\"" + calendarEvent.Description + "\"";
            else
                sql += ",\"\"";
            
            sql += ");";
            
            stmt.executeUpdate(sql);
            stmt.close();
      
            System.out.println("Created calendar event: " + calendarEvent.Header);
        }
        catch(Exception e)
        {
            System.out.println("Exception (Create Calendar Event): " + e.getMessage());
        }
         
        PostAction();
        
        return result;
    }
    public boolean EditCalendarEvent(CalendarEvent calendarEvent)
    {
        System.out.println("Editing calendar event: " + calendarEvent.Header);
        
        boolean result = true;
        
        PreAction();
        
        try
        {
            Statement stmt = connection.createStatement();
            String sql = 
                    "UPDATE CalendarEvents SET ";
            
            if (calendarEvent.CreatorID != null)
                sql += "CreatorID = \"" + calendarEvent.CreatorID + "\" , ";
            else
                sql += "CreatorID = \"\" , ";
            
            if (calendarEvent.StartDate != null)
                sql += "StartDate = \"" + calendarEvent.StartDate + "\" , ";
            else
                sql += "StartDate = \"\" , ";
            
            if (calendarEvent.EndDate != null)
                sql += "EndDate = \"" + calendarEvent.EndDate + "\" , ";
            else
                sql += "EndDate = \"\" , ";
            
            if (calendarEvent.Header != null)
                sql += "Header = \"" + calendarEvent.Header + "\" , ";
            else
                sql += "Header = \"\" , ";
            
            if (calendarEvent.Description != null)
                sql += "Description = \"" + calendarEvent.Description + "\" ";
            else
                sql += "Description = \"\" ";
            
            sql += " WHERE UniqueID=" + calendarEvent.UniqueID + ";";
            
            stmt.executeUpdate(sql);
            stmt.close();
            
            System.out.println("Edited calendar event: " + calendarEvent.Header);
        }
        catch(Exception e)
        {
            System.out.println("Exception (Edit Calendar Event): " + e.getMessage());
        }
        
        PostAction();
        
        return result;
    }
    public boolean DeleteCalendarEvent(CalendarEvent calendarEvent)
    {
        System.out.println("Deleting calendar event: " + calendarEvent.Header);
        
        boolean result = true;
        
        PreAction();
        
        try 
        {
            Statement stmt = connection.createStatement();
            String sql = "DELETE from CalendarEvents where UniqueID=" 
                    + calendarEvent.UniqueID + ";";
            stmt.executeUpdate(sql);
            
            System.out.println("Deleted calendar event: " + calendarEvent.Header);
        }
        catch(Exception e)
        {
            System.out.println("Exception (Delete Calendar Event): " + e.getMessage());
        }    
        
        PostAction();
        
        return result;
    }
    public CalendarEvent[] GetCalendarEvents()
    {
        System.out.println("Getting calendar events.");
        
        ArrayList<CalendarEvent> calendarEventsList = new ArrayList<>();

        PreAction();

        try 
        {
            Statement stmt = connection.createStatement();
            ResultSet rs = stmt.executeQuery("SELECT * FROM CalendarEvents;");

            while (rs.next()) 
            {
                int     UniqueID        = rs.getInt("UniqueID");
                String  CreatorID       = rs.getString("CreatorID");
                String  StartDate       = rs.getString("StartDate");
                String  EndDate         = rs.getString("EndDate");
                String  Header          = rs.getString("Header");
                String  Description     = rs.getString("Description");
                
                calendarEventsList.add(new CalendarEvent(   UniqueID,
                                                            CreatorID,
                                                            StartDate,
                                                            EndDate,
                                                            Header,
                                                            Description));
            }   
            rs.close();
            stmt.close();
            
            System.out.println("Got calendar events.");
        }
        catch(Exception e)
        {
            System.out.println("Exception (Get Calendar Events): " + e.getMessage());
        }
 
        PostAction();
        
        return calendarEventsList.toArray(new CalendarEvent[calendarEventsList.size()]);
    }
    
    public boolean CreateUser(CalendarUser calendarUser)
    {
        System.out.println("Creating user: " + calendarUser.IPAddress);
        
        boolean result = true;
        
        PreAction();
        
        try
        {
            Statement stmt = connection.createStatement();
            String sql = "INSERT INTO CalendarUsers" + 
                    "(IPAddress) VALUES (\"" + calendarUser.IPAddress +  "\");";
            stmt.executeUpdate(sql);
            stmt.close();
            System.out.println("Created user: " + calendarUser.IPAddress);     
        }
        catch(Exception e)
        {
            System.out.println("Exception (Create User): " + e.getMessage());
        }
        
        PostAction();
        
        return result;
    }
    public boolean DeleteUser(CalendarUser calendarUser)
    {
        System.out.println("Deleting user: " + calendarUser.IPAddress);
        
        boolean result = true;
        
        PreAction();
        
        try 
        {
            Statement stmt = connection.createStatement();
            String sql = "DELETE from CalendarUsers where IPAddress=\"" 
                    + calendarUser.IPAddress + "\";";
            stmt.executeUpdate(sql);
            System.out.println("Deleted user: " + calendarUser.IPAddress);  
        }
        catch(Exception e)
        {
            System.out.println("Exception (Delete User): " + e.getMessage());
        }    
        
        PostAction();
        
        return result;
    }
    public CalendarUser[] GetUsers()
    {
        System.out.println("Getting users...");
        
        ArrayList<CalendarUser> calendarUsersList = new ArrayList<>();

        PreAction();

        try 
        {
            Statement stmt = connection.createStatement();
            ResultSet rs = stmt.executeQuery("SELECT * FROM CalendarUsers;");

            while (rs.next()) 
            {
                String  IPAddress = rs.getString("IPAddress");
                calendarUsersList.add(new CalendarUser(IPAddress));
            }   
            
            rs.close();
            stmt.close();
            
            System.out.println("Got users.");
        }
        catch(Exception e)
        {
            System.out.println("Exception (Get Users): " + e.getMessage());
        }
 
        PostAction();
        
        return calendarUsersList.toArray(new CalendarUser[calendarUsersList.size()]);
    }
    
    public void ClearCalendarEvents()
    {
        System.out.println("Clearing calendar events table.");
        
        PreAction();
        try 
        {
            Statement stmt = connection.createStatement();
            String sql = "DROP TABLE CalendarEvents";
            stmt.executeUpdate(sql);
            stmt.close();
            System.out.println("Cleared calendar events table.");
        }
        catch(Exception e)
        {
            System.out.println("Exception (Clear Calendar Events): " + e.getMessage());
        }
        
        PostAction();
    }
    public void ClearUsers()
    {
        System.out.println("Clearing calendar users table.");
        
        PreAction();
        
        try 
        {
            Statement stmt = connection.createStatement();
            String sql = "DROP TABLE CalendarUsers";
            stmt.executeUpdate(sql);
            stmt.close();
            System.out.println("Cleared calendar users table.");
        }
        catch(Exception e)
        {
            System.out.println("Exception (Clear Calendar Users): " + e.getMessage());
        }
        
        PostAction();
    }
    public void CreateUsers(CalendarUser[] users)
    {
        for(int i = 0; i < users.length; i++)
            CreateUser(users[i]);
    }
    public void CreateCalendarEvents(CalendarEvent[] calendarEvents)
    {
        for(int i = 0; i < calendarEvents.length; i++)
            CreateCalendarEvent(calendarEvents[i]);
    }
    
}
