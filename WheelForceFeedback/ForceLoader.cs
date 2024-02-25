using System.Text.Json;
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
        using var messageServer = new ResponseSocket("@tcp://localhost:5556");

        LoadForceEffects();

        while (true)
        {
            string message = messageServer.ReceiveFrameString();
            int type = (int)Char.GetNumericValue(message[0]);
            
            if (type == (int)MessageType.Rumble)
            {
                ActivateRumbleMessage rumbleMessage = JsonSerializer.Deserialize<ActivateRumbleMessage>(message.Substring(1));
                Console.WriteLine("rumble");
                rumbleWheel.Start();
            }

            if (type == (int)MessageType.Setting)
            {
                feedbackSettings = JsonSerializer.Deserialize<FeedbackSettings>(message.Substring(1));
            }
            
            messageServer.SendFrame("message received");
        }
    }

    async void LoadCenteringForce()
    {
    }

    async void LoadRumbleForce(ActivateRumbleMessage parameters)
    {
        racingWheel.WheelMotor.TryUnloadEffectAsync(rumbleWheel);
        
        rumbleWheel = new(PeriodicForceEffectKind.SineWave);
        rumbleWheel.SetParameters(new(feedbackSettings.RumbleForce, 0, 0), 0.5f, 0.5f, 0.5f, TimeSpan.FromSeconds(1));
    }

    async void LoadForceEffects()
    {
        turnWheelLeft = new();
        turnWheelRight = new();
        rumbleWheel = new(PeriodicForceEffectKind.TriangleWave);
        turnWheelLeft.SetParameters(new(feedbackSettings.CenterSpringForce, 0, 0), TimeSpan.FromSeconds(1));
        turnWheelRight.SetParameters(new(-feedbackSettings.CenterSpringForce, 0, 0), TimeSpan.FromSeconds(1));
        rumbleWheel.SetParameters(new(0.5f, 0, 0), feedbackSettings.RumbleFrequency, 0.5f, 0f, TimeSpan.FromSeconds(1));
        rumbleWheel.Gain = feedbackSettings.RumbleForce;
        
        IAsyncOperation<ForceFeedbackLoadEffectResult> loadLeftRequest =
            racingWheel.WheelMotor.LoadEffectAsync(turnWheelLeft);

        ForceFeedbackLoadEffectResult result = await loadLeftRequest.AsTask();

        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("left effect loaded");

        IAsyncOperation<ForceFeedbackLoadEffectResult> loadRightRequest =
            racingWheel.WheelMotor.LoadEffectAsync(turnWheelRight);

        result = await loadRightRequest.AsTask();

        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("right effect loaded");

        IAsyncOperation<ForceFeedbackLoadEffectResult> loadRumbleRequest =
            racingWheel.WheelMotor.LoadEffectAsync(rumbleWheel);

        result = await loadRumbleRequest.AsTask();

        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("rumble effect loaded");
    }

    void UnLoadForceEffects()
    {
        racingWheel.WheelMotor.TryUnloadEffectAsync(turnWheelLeft);
        racingWheel.WheelMotor.TryUnloadEffectAsync(turnWheelRight);
    }
}