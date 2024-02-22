
using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.Gaming.Input.ForceFeedback;

namespace WheelForceFeedback;

public static class Program
{
    public static void Main(string[] args)
    {
        while (RacingWheel.RacingWheels.Count == 0)
        {
            
        }
    }
    
    async void LoadForceEffects()
    {
        turnWheelLeft = new ();
        turnWheelRight = new ();
        rumbleWheel = new (PeriodicForceEffectKind.SineWave);
        turnWheelLeft.SetParameters(new (centerSpringForce, 0, 0), TimeSpan.FromSeconds(10));
        turnWheelRight.SetParameters(new (-centerSpringForce, 0, 0), TimeSpan.FromSeconds(10));
        rumbleWheel.SetParameters(new (rumbleForce, 0, 0), 0.5f, 0.5f, 0.5f, TimeSpan.FromSeconds(10));

        IAsyncOperation<ForceFeedbackLoadEffectResult> loadLeftRequest = racingWheel.WheelMotor.LoadEffectAsync(turnWheelLeft);

        ForceFeedbackLoadEffectResult result = await loadLeftRequest.AsTask();

        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("left effect loaded");

        IAsyncOperation<ForceFeedbackLoadEffectResult> loadRightRequest = racingWheel.WheelMotor.LoadEffectAsync(turnWheelRight);

        result = await loadRightRequest.AsTask();

        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("right effect loaded");

        IAsyncOperation<ForceFeedbackLoadEffectResult> loadRumbleRequest = racingWheel.WheelMotor.LoadEffectAsync(rumbleWheel);

        result = await loadRumbleRequest.AsTask();

        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("rumble effect loaded");
    }
}