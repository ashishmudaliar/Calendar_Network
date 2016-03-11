using System;
using BitCalendar.Presenter;
using BitCalendarService;

namespace BitCalendar.View
{
    class CalendarViewConsole
    {
        public CalendarPresenterConsole Presenter;

        public CalendarViewConsole()
        {
            
        }

        public void Run()
        {
            var userInput = Console.ReadLine();
            while (userInput != null)
            {
                Presenter.ParseInput(userInput);
                userInput = Console.ReadLine();
            }
        }

        public void PrintLine(String output)
        {
            Console.WriteLine(output);
        }

        public void PrintCalendarEvent(CalendarEvent calendarEvent)
        {
            Console.WriteLine(
                          "- Unique ID:   " + calendarEvent.UniqueID    + "\n"
                        + "- Author:      " + calendarEvent.CreatorID   + "\n"
                        + "- Start Date:  " + calendarEvent.StartDate   + "\n"
                        + "- End Date:    " + calendarEvent.EndDate     + "\n" 
                        + "- Header:      " + calendarEvent.Header      + "\n" 
                        + "- Description: " + calendarEvent.Description);
        }
    }
}
