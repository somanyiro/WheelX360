using Windows.Gaming.Input;

namespace WheelForceFeedback;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("searching for racing wheel");
        while (RacingWheel.RacingWheels.Count == 0)
        {
            Thread.Sleep(100);
        }
        Console.WriteLine("racing wheel found");

        new ForceLoader().Run();
    }
}