import java.util.Date;
import java.util.HashMap;
import java.util.Map;

public class CalendarEvent 
{
    public int      UniqueID;
    public String   CreatorID;
    public String   StartDate;
    public String   EndDate;
    public String   Header;
    public String   Description;
    
    public CalendarEvent()
    {
        UniqueID = -1;
        CreatorID = "";
        StartDate = "";
        EndDate = "";
        Header = "";
        Description = "";
    }
    public CalendarEvent(Map map)
    {
        SetFromMap(map);
    }
    public CalendarEvent(String string)
    {
        DecodeFromString(string);
    }
    public CalendarEvent(int     uniqueID, 
                         String  creatorID, 
                         String  startDate,
                         String  endDate,
                         String  header,
                         String  description)
    {
        UniqueID    = uniqueID;
        CreatorID   = creatorID;
        StartDate   = startDate;
        EndDate     = endDate;
        Header      = header;
        Description = description;
    }
    
    public void SetFromMap(Map map)
    {
        UniqueID    = (int)       map.get("UniqueID");
        CreatorID   = (String)    map.get("CreatorID");
        StartDate   = (String)    map.get("StartDate");
        EndDate     = (String)    map.get("EndDate");
        Header      = (String)    map.get("Header");
        Description = (String)    map.get("Description");
    }
    public Map GetAsMap()
    {
        HashMap map = new HashMap();
       
        map.put("UniqueID"      , UniqueID);
        map.put("CreatorID"     , CreatorID);
        map.put("StartDate"     , StartDate);
        map.put("EndDate"       , EndDate);
        map.put("Header"        , Header);
        map.put("Description"   , Description);
        
        return map;
    }
    
    public String EncodeToString() 
    {
        return UniqueID + "|||" + CreatorID + "|||" + StartDate + "|||"
                + EndDate + "|||" + Header + "|||" + Description;
    }
    public boolean DecodeFromString(String input) 
    {
        String[] splitArray = input.split("\\|\\|\\|");

        if (splitArray.length != 6)
        {
            System.out.println("Calendar event decode unsuccessful.");
            System.out.println("String is: " + input);
            return false;
        }
        
        int uniqueID = -1;
        try
        {
            uniqueID = Integer.parseInt(splitArray[0]);
            UniqueID = uniqueID;
        }
        catch (Exception e)
        {
            UniqueID = -1;
        }
        
        if(splitArray.length >= 2)
            CreatorID = splitArray[1];
        if(splitArray.length >= 3)
            StartDate = splitArray[2];
        if(splitArray.length >= 4)
            EndDate = splitArray[3];
        if(splitArray.length >= 5)
            Header = splitArray[4];
        if(splitArray.length >= 6)
            Description = splitArray[5];

        return true;
    }
}
