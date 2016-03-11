import java.util.Scanner;

public class Program 
{ 
    public static void main(String[] args) 
    {
        if (args.length > 0)
        {
            String stringPort = args[0];
            try
            {
                CalendarNetworkManager.Port = Integer.parseInt(stringPort); 
            }
            catch(Exception e)
            {
                CalendarNetworkManager.Port = 8080;
            	System.out.println("Invalid port! Using default (8080).");
            }
        }
        else
        {
            System.out.println("Please specify the client port: ");
            Scanner scanner = new Scanner(System.in);
            String userInput = scanner.nextLine();
            try
            {
                CalendarNetworkManager.Port = Integer.parseInt(userInput); 
            }
            catch(Exception e)
            {
                CalendarNetworkManager.Port = 8080;
            	System.out.println("Invalid port! Using default (8080).");
            }       
        }    	
        
        CalendarCore core = new CalendarCore();
        CalendarView view = new CalendarView();
        CalendarPresenter presenter = new CalendarPresenter(core,view);
        view.Run();
    }
}
