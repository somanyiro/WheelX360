using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.Gaming.Input.ForceFeedback;
using NetMQ;
using SharedClasses;
using NetMQ.Sockets;

namespace WheelForceFeedback;

public class ForceLoader
{
    private ConstantForceEffect turnWheelLeft = new ConstantForceEffect();
    private ConstantForceEffect turnWheelRight = new ConstantForceEffect();
    private PeriodicForceEffect rumbleWheel = new PeriodicForceEffect(PeriodicForceEffectKind.SineWave);

    private FeedbackSettings feedbackSettings = new FeedbackSettings();
    
    RacingWheel racingWheel = RacingWheel.RacingWheels[0];
    
    public void Run()
    {
        using var server = new ResponseSocket("@tcp://localhost:5556");
        
        while (true)
        {
            string m1 = server.ReceiveFrameString();
            Console.WriteLine("From Client: {0}", m1);
            server.SendFrame("hi back");
        }
        
        
        
        //LoadForceEffects();

    }
    
    async void LoadForceEffects()
    {
        turnWheelLeft = new ();
        turnWheelRight = new ();
        rumbleWheel = new (PeriodicForceEffectKind.SineWave);
        turnWheelLeft.SetParameters(new (feedbackSettings.centerSpringForce, 0, 0), TimeSpan.FromSeconds(10));
        turnWheelRight.SetParameters(new (-feedbackSettings.centerSpringForce, 0, 0), TimeSpan.FromSeconds(10));
        rumbleWheel.SetParameters(new (feedbackSettings.rumbleForce, 0, 0), 0.5f, 0.5f, 0.5f, TimeSpan.FromSeconds(10));

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
    
    void UnLoadForceEffects()
    {
        racingWheel.WheelMotor.TryUnloadEffectAsync(turnWheelLeft);
        racingWheel.WheelMotor.TryUnloadEffectAsync(turnWheelRight);
        racingWheel.WheelMotor.TryUnloadEffectAsync(rumbleWheel);
    }
}