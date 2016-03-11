using System;

namespace BitCalendarService
{
    class ExtendedTimestamp
    {
        public int      Time                { get; private set; }
        public string   SenderIP            { get; private set; }
        public int      CriticalRegionNo    { get; private set; }

        public ExtendedTimestamp(int time, string senderIP, int criticalRegionNo)
        {
            Time                = time;
            SenderIP            = senderIP;
            CriticalRegionNo    = criticalRegionNo;
        }

        public string ToString()
        {
            return Time + "#" + SenderIP + "#" + CriticalRegionNo;
        }
        
        public static ExtendedTimestamp FromString(string timestampString)
        {
            var splitArray = timestampString.Split(new[] {"#"}, StringSplitOptions.None);
            if (splitArray.Length != 3)
            {
                Console.WriteLine("Timestamp decode failed!");
                return null;
            }

            return new ExtendedTimestamp(int.Parse(splitArray[0]),splitArray[1],int.Parse(splitArray[2]));
        }
    }
}
