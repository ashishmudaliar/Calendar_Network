using System;

namespace BitCalendarService
{
    class LamportClock
    {
        public int CurrentTime;

        public LamportClock()
        {
            CurrentTime = 0;
        }

        public int Increment()
        {
            CurrentTime ++;
            Console.WriteLine("Incremented Lamport Clock: " + CurrentTime);
            return CurrentTime;
        }
        public int Adjust(int senderCurrentTime)
        {
            CurrentTime = Math.Max(CurrentTime, senderCurrentTime) + 1;
            Console.WriteLine("Adjusted Lamport Clock: " + CurrentTime);
            return CurrentTime;
        }
        public void Reset()
        {
            CurrentTime = 0;
        }
    }
}
