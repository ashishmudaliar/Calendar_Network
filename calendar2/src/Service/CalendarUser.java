import java.util.HashMap;
import java.util.Map;

public class CalendarUser 
{
    public String IPAddress;
    
    public CalendarUser()
    {
        
    }
    public CalendarUser(Map map)
    {
        SetFromMap(map);
    }
    public CalendarUser(String ipAddress)
    {
        DecodeFromString(ipAddress);
    }
    
    public void SetFromMap(Map map)
    {
        IPAddress = (String) map.get("IPAddress");
    }
    public Map GetAsMap()
    {
        HashMap map = new HashMap();
       
        map.put("IPAddress", IPAddress);
        
        return map;
    }
    
    public String EncodeToString() 
    {
        return IPAddress;
    }
    public boolean DecodeFromString(String input) 
    {
        if(input == null || input.trim().equals(new String()))
            return false;
        
        IPAddress = input;
        return true;
    }
}
