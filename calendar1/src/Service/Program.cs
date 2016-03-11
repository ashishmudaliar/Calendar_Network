using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Microsoft.Samples.XmlRpc;

namespace BitCalendarService
{
    class Program
    {
        public  static int          Port = 8080;
        public  static string       BaseAddress = "/BitCalendarService";
        private static int          _mode = 0; // 0 = Ricart & Agrawala, 1 = Token Ring
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var stringPort = args[0];
                int numericPort;
                if (!int.TryParse(stringPort, out numericPort))
                    Console.WriteLine("Invalid port! Using default (8080).");
                else
                    Port = numericPort; 
            }
            else
            {
                Console.WriteLine("Please specify the service port: ");
                var stringPort = Console.ReadLine();
                int numericPort;
                if (!int.TryParse(stringPort, out numericPort))
                    Console.WriteLine("Invalid port! Using default (8080).");
                else
                    Port = numericPort;        
            }

            if (args.Length > 1)
            {
                var stringMode = args[1];
                int numericMode;
                if (!int.TryParse(stringMode, out numericMode))
                    Console.WriteLine("Invalid mode! Using default (Ricart & Agrawala).");
                else
                    _mode = numericMode; 
            }
            else
            {
                Console.WriteLine("Please specify the algorithm to be used (0 for Ricart & Agrawala, 1 for Token Ring): ");
                var stringMode = Console.ReadLine();
                int numericMode;
                if (!int.TryParse(stringMode, out numericMode))
                    Console.WriteLine("Invalid mode! Using default (0 - Ricart & Agrawala).");
                else
                    _mode = numericMode;
            }

            Console.WriteLine(_mode == 0 ? "Mode: Ricart & Agrawala" : "Mode: Token Ring");

            Console.WriteLine("Launching calendar service.");
            var baseAddress = new Uri("http://" + CalendarServiceUtility.IPAndPort + BaseAddress);

            ServiceHost serviceHost;
            if(_mode == 0)
                serviceHost = new ServiceHost(typeof(CalendarServiceRicartAgrawala));
            else
                serviceHost = new ServiceHost(typeof(CalendarServiceTokenRing));

            var epXmlRpc = serviceHost.AddServiceEndpoint(typeof(ICalendarService),
                new WebHttpBinding(WebHttpSecurityMode.None), new Uri(baseAddress, ""));
            epXmlRpc.Behaviors.Add(new XmlRpcEndpointBehavior());

            serviceHost.Open();
            
            Console.WriteLine("B-IT Calendar service endpoint listening at  {0}", epXmlRpc.ListenUri);
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();

            serviceHost.Close();
        }
    }
}
