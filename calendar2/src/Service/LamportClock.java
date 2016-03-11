
public class LamportClock {

    public int CurrentTime;

    public LamportClock() {
        CurrentTime = 0;
    }

    public int Increment() {
        CurrentTime++;
        System.out.println("Incremented Lamport Clock: " + CurrentTime);
        return CurrentTime;
    }

    public int Adjust(int senderCurrentTime) {
        CurrentTime = Math.max(CurrentTime, senderCurrentTime) + 1;
        System.out.println("Adjusted Lamport Clock: " + CurrentTime);
        return CurrentTime;
    }

    public void Reset() {
        CurrentTime = 0;
    }
}
