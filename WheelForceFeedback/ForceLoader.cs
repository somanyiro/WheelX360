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
    private PeriodicForceEffect rumbleWheel;
    private ConditionForceEffect centerWheel;
    
    private FeedbackSettings feedbackSettings = new FeedbackSettings();
    
    RacingWheel racingWheel = RacingWheel.RacingWheels[0];

    public void Run()
    {
        using var messageServer = new ResponseSocket("@tcp://localhost:5556");

        racingWheel.WheelMotor.TryResetAsync();
        LoadForceEffects();
        
        while (true)
        {
            string message = messageServer.ReceiveFrameString();
            int type = (int)Char.GetNumericValue(message[0]);
            
            if (type == (int)MessageType.Rumble)
            {
                ActivateRumbleMessage rumbleMessage = JsonSerializer.Deserialize<ActivateRumbleMessage>(message.Substring(1));
                rumbleWheel.Gain = feedbackSettings.RumbleForce * Utils.Map(
                    (int)rumbleMessage.LargeMotor + (int)rumbleMessage.SmallMotor,
                    0, 510, 0, 1);
            }

            if (type == (int)MessageType.Setting)
            {
                feedbackSettings = JsonSerializer.Deserialize<FeedbackSettings>(message.Substring(1));
                rumbleWheel.SetParameters(new(0.5f, 0, 0), feedbackSettings.RumbleFrequency, 0.5f, 0f, TimeSpan.MaxValue);
                centerWheel.SetParameters(new(0.5f, 0, 0), 1, 1, 1, 1, feedbackSettings.CenterSpringDeadzone/90, 0);
                centerWheel.Gain = feedbackSettings.CenterSpringForce;
            }
            
            messageServer.SendFrame("message received");
        }
    }

    async void LoadForceEffects()
    {
        centerWheel = new(ConditionForceEffectKind.Spring);
        rumbleWheel = new(PeriodicForceEffectKind.TriangleWave);
        centerWheel.SetParameters(new(0.5f, 0, 0), 1, 1, 1, 1, feedbackSettings.CenterSpringDeadzone/90, 0);
        rumbleWheel.SetParameters(new(0.5f, 0, 0), feedbackSettings.RumbleFrequency, 0.5f, 0f, TimeSpan.MaxValue);

        IAsyncOperation<ForceFeedbackLoadEffectResult> loadCenterRequest =
            racingWheel.WheelMotor.LoadEffectAsync(centerWheel);

        ForceFeedbackLoadEffectResult result = await loadCenterRequest.AsTask();
        
        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("center effect loaded");
        
        centerWheel.Start();

        IAsyncOperation<ForceFeedbackLoadEffectResult> loadRumbleRequest =
            racingWheel.WheelMotor.LoadEffectAsync(rumbleWheel);

        result = await loadRumbleRequest.AsTask();

        if (result == ForceFeedbackLoadEffectResult.Succeeded)
            Console.WriteLine("rumble effect loaded");

        rumbleWheel.Gain = 0;
        rumbleWheel.Start();
    }

    void UnLoadForceEffects()
    {
        racingWheel.WheelMotor.TryUnloadEffectAsync(centerWheel);
        racingWheel.WheelMotor.TryUnloadEffectAsync(rumbleWheel);
    }
}