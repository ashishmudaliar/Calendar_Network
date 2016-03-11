public class ExtendedTimestamp 
{

    public int Time;
    public String SenderIP;
    public int CriticalRegionNo;

    public ExtendedTimestamp(int time, String senderIP, int criticalRegionNo) 
    {
        Time = time;
        SenderIP = senderIP;
        CriticalRegionNo = criticalRegionNo;
    }

    public String ToString() 
    {
        return Time + "#" + SenderIP + "#" + CriticalRegionNo;
    }

    public static ExtendedTimestamp FromString(String timestampString) 
    {
        String[] splitArray = timestampString.split("#");

        if (splitArray.length != 3) {
            System.out.println("Timestamp decode failed!");
            return null;
        }

        return new ExtendedTimestamp(Integer.parseInt(splitArray[0]), splitArray[1], Integer.parseInt(splitArray[2]));
    }
}
