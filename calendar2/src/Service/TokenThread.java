
public class TokenThread implements Runnable
{
    public void run() 
    {
        CalendarServiceTokenRing.TokenCoroutine();
    }   
}
