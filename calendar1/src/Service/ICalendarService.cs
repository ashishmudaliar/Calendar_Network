using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace BitCalendarService
{
    [ServiceContract]
    public interface ICalendarService
    {
        [OperationContract (Action = "BitCalendarService.CreateCalendarEvent")]
        bool CreateCalendarEvent(string calendarEventString);
        [OperationContract(Action = "BitCalendarService.EditCalendarEvent")]
        bool EditCalendarEvent(string calendarEventString);
        [OperationContract(Action = "BitCalendarService.DeleteCalendarEvent")]
        bool DeleteCalendarEvent(string calendarEventString);
        [OperationContract(Action = "BitCalendarService.GetCalendarEvents")]
        string GetCalendarEvents();
        [OperationContract(Action = "BitCalendarService.GetUsers")]
        string GetUsers();

        [OperationContract(Action = "BitCalendarService.Connect")]
        bool Connect(string targetIPAddress);
        [OperationContract(Action = "BitCalendarService.Disconnect")]
        bool Disconnect();
        [OperationContract(Action = "BitCalendarService.RegisterUser")]
        bool RegisterUser(string calendarUserIPString);
        [OperationContract(Action = "BitCalendarService.DeregisterUser")]
        bool DeregisterUser(string calendarUserIPString);

        [OperationContract(Action = "BitCalendarService.ClearCalendarEventsTable")]
        bool ClearCalendarEventsTable();
        [OperationContract(Action = "BitCalendarService.ClearUsersTable")]
        bool ClearUsersTable();

        [OperationContract(Action = "BitCalendarService.CreateCalendarEventInternal")]
        bool CreateCalendarEventInternal(string calendarEventString);
        [OperationContract(Action = "BitCalendarService.EditCalendarEventInternal")]
        bool EditCalendarEventInternal(string calendarEventString);
        [OperationContract(Action = "BitCalendarService.DeleteCalendarEventInternal")]
        bool DeleteCalendarEventInternal(string calendarEventString);

        [OperationContract(Action = "BitCalendarService.Initialize")]
        bool Initialize();
        [OperationContract(Action = "BitCalendarService.RequestLock")]
        string RequestLock(string timestampString);
        [OperationContract(Action = "BitCalendarService.PassToken")]
        bool PassToken();
        [OperationContract(Action = "BitCalendarService.ReceiveToken")]
        bool ReceiveToken();
        [OperationContract(Action = "BitCalendarService.DebugLock")]
        bool DebugLock();
        [OperationContract(Action = "BitCalendarService.DebugUnlock")]
        bool DebugUnlock();

    }

    //[DataContract]
    public class CalendarEvent
    {
        //[DataMember]
        public int UniqueID;
        //[DataMember]
        public string CreatorID;
        //[DataMember]
        public string StartDate;
        //[DataMember]
        public string EndDate;
        //[DataMember]
        public string Header;
        //[DataMember]
        public string Description;

        public CalendarEvent()
        {
            
        }
        public CalendarEvent(string input)
        {
            DecodeFromString(input);
        }
        public CalendarEvent(int uniqueID,
                         String creatorID,
                         String startDate,
                         String endDate,
                         String header,
                         String description)
        {
            UniqueID = uniqueID;
            CreatorID = creatorID;
            StartDate = startDate;
            EndDate = endDate;
            Header = header;
            Description = description;
        }

        public string EncodeToString()
        {
            return UniqueID + "|||" + CreatorID + "|||" + StartDate + "|||" +
                   EndDate  + "|||" + Header    + "|||" + Description;
        }
        public bool   DecodeFromString(string input)
        {
            string[] stringSeperators = new string[]{"|||"};
            string[] splitList = input.Split(stringSeperators, StringSplitOptions.None);
            if (splitList.Length != 6)
            {
                Console.WriteLine("Calendar event decode unsuccessful.");
                Console.WriteLine("String is: " + input);
                return false;
            }
            
            int uniqueID = -1;
            try
            {
                uniqueID = int.Parse(splitList[0]);
                UniqueID = uniqueID;
            }
            catch (Exception)
            {
                Console.Write("Calendar event decode warning (Unique ID set to -1).");
                UniqueID = -1;
            }
            
            if(splitList.Length >= 2)
                CreatorID   = splitList[1];
            if (splitList.Length >= 3)
                StartDate   = splitList[2];
            if (splitList.Length >= 4)
                EndDate     = splitList[3];
            if (splitList.Length >= 5)
                Header      = splitList[4];
            if (splitList.Length >= 6)
                Description = splitList[5];

            return true;
        }
    }

    //[DataContract]
    public class CalendarUser
    {
        //[DataMember]
        public string IPAddress;

        public CalendarUser()
        {
            
        }
        public CalendarUser(string input)
        {
            DecodeFromString(input);
        }

        public string EncodeToString()
        {
            return IPAddress;
        }
        public bool DecodeFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            IPAddress = input;
            return true;
        }
    }
}
