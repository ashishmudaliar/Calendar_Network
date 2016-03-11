import java.util.Scanner;


public class CalendarView 
{
    public CalendarPresenter presenter;
    
    public CalendarView()
    {
        
    }
    
    public void Run()
    {
        Scanner scanner = new Scanner(System.in);
        String userInput = scanner.nextLine();
        
        while (userInput != null && !userInput.equals(""))
        {
            presenter.ParseInput(userInput);
            
            userInput = scanner.nextLine();
        }   
    }
    
    public void PrintLine(String output)
    {
        System.out.println(output);
    }
    
    public void PrintCalendarEvent(CalendarEvent calendarEvent)
    {
        System.out.println(
                          "- Unique ID:   " + calendarEvent.UniqueID    + "\n"
                        + "- Author:      " + calendarEvent.CreatorID   + "\n"
                        + "- Start Date:  " + calendarEvent.StartDate   + "\n"
                        + "- End Date:    " + calendarEvent.EndDate     + "\n" 
                        + "- Header:      " + calendarEvent.Header      + "\n" 
                        + "- Description: " + calendarEvent.Description);
    }
}
