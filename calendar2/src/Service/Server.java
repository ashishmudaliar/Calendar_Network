import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.util.Enumeration;
import java.util.Scanner;
import org.apache.xmlrpc.server.PropertyHandlerMapping;
import org.apache.xmlrpc.server.XmlRpcServer;
import org.apache.xmlrpc.server.XmlRpcServerConfigImpl;
import org.apache.xmlrpc.webserver.WebServer;

public class Server 
{
    public  static int      Port        = 8080;
    public  static String   BaseAddress = "/BitCalendarService";
    private static int      _mode       = 0;
    
    public static String IP() 
    {
        String ip = null;
        int flag = 0;
        try 
        {
            Enumeration<NetworkInterface> interfaces = NetworkInterface.getNetworkInterfaces();
            while (interfaces.hasMoreElements() && flag == 0)
            {
                NetworkInterface iface = interfaces.nextElement();
                
                // Filters out 127.0.0.1 and inactive interfaces
                if (iface.isLoopback() || !iface.isUp())
                    continue;

                Enumeration<InetAddress> addresses = iface.getInetAddresses();
                while (addresses.hasMoreElements()) {
                    InetAddress addr = addresses.nextElement();
                    if (addr instanceof Inet4Address) {
                        ip = addr.getHostAddress();
                        flag = 1;
                    }
                }
            }
            return ip;
        } 
        catch (Exception e) 
        {
            return "Error retrieving IP address.";
        }
    }
    public static String IPAndPort()
    {
        return IP() + ":" + Port;
    }
    
    public static void main(String[] args) throws Exception 
    {
        ProcessAndPromptArguments(args);
        
        WebServer webServer = new WebServer(Port);
        XmlRpcServer xmlRpcServer = webServer.getXmlRpcServer();
        PropertyHandlerMapping phm = new PropertyHandlerMapping();
        
        if(_mode == 0)
            phm.addHandler("BitCalendarService", CalendarServiceRicartAgrawala.class);     
        else
            phm.addHandler("BitCalendarService", CalendarServiceTokenRing.class);      
        
        xmlRpcServer.setHandlerMapping(phm);
        
        XmlRpcServerConfigImpl serverConfig =
            (XmlRpcServerConfigImpl) xmlRpcServer.getConfig();
        serverConfig.setEnabledForExtensions(true);
        serverConfig.setContentLengthOptional(false);
        
        webServer.start();
        
        //webServer.shutdown();
    }

    public static void ProcessAndPromptArguments(String[] args)
    {
        if (args.length > 0)
        {
            String stringPort = args[0];
            try
            {
                Port = Integer.parseInt(stringPort); 
                System.out.println("Port is: " + Port + ".");
            }
            catch(Exception e)
            {
                Port = 8080;
            	System.out.println("Invalid port! Using default (8080).");
            }
        }
        else
        {
            System.out.println("Please specify the service port: ");
            Scanner scanner = new Scanner(System.in);
            String userInput = scanner.nextLine();
            try
            {
                Port = Integer.parseInt(userInput); 
                System.out.println("Port is: " + Port + ".");
            }
            catch(Exception e)
            {
                Port = 8080;
            	System.out.println("Invalid port! Using default (8080).");
            }       
        }    
        
        if (args.length > 1)
        {
            String stringMode = args[1];
            try
            {
                _mode = Integer.parseInt(stringMode); 
            }
            catch(Exception e)
            {
                _mode = 0;
            	System.out.println("Invalid mode! Using default (Ricart & Agrawala).");
            }
        }
        else
        {
        	System.out.println("Please specify the Algorithm to be used (0 for Ricart & Agarwala, 1 for Token Ring): ");
        	Scanner scanner = new Scanner(System.in);
        	String userInput = scanner.nextLine();
        	_mode = Integer.parseInt(userInput);
        }
        
        System.out.println(_mode == 0 ? "Mode: Ricart & Agrawala" : "Mode: Token Ring");

        
    }
}
