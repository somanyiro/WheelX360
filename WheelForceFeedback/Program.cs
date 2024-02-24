using Windows.Gaming.Input;

namespace WheelForceFeedback;

public static class Program
{
    public static void Main(string[] args)
    {
        while (RacingWheel.RacingWheels.Count == 0)
        {
            Console.WriteLine("searching for racing wheel");
            Thread.Sleep(100);
        }
        Console.WriteLine("racing wheel found");

        new ForceLoader().Run();
    }
}