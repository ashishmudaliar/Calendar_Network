using System;
using System.Windows.Forms;
using BitCalendar.Core;
using BitCalendar.Core.Network;
using BitCalendar.Presenter;
using BitCalendar.View;

namespace BitCalendar
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var stringPort = args[0];
                int numericPort;
                if (!int.TryParse(stringPort, out numericPort))
                    Console.WriteLine("Invalid port! Using default (8080).");
                else
                    CalendarNetworkManager.Port = numericPort;
            }
            else
            {
                Console.WriteLine("Please specify the client port: ");
                var stringPort = Console.ReadLine();
                int numericPort;
                if (!int.TryParse(stringPort, out numericPort))
                    Console.WriteLine("Invalid port! Using default (8080).");
                else
                    CalendarNetworkManager.Port = numericPort;    
            }
            
            var core = new CalendarCore();
            var view = new CalendarViewConsole();
            var calendarPresenter = new CalendarPresenterConsole(core, view);
            view.Run();
        }
    }
}
