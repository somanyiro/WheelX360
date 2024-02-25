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
    private PeriodicForceEffect rumbleWheel = new PeriodicForceEffect(PeriodicForceEffectKind.TriangleWave);
    
    private FeedbackSettings feedbackSettings = new FeedbackSettings();
    
    RacingWheel racingWheel = RacingWheel.RacingWheels[0];

    public void Run()
    {
        using var messageServer = new ResponseSocket("@tcp://localhost:5556");

        racingWheel.WheelMotor.TryResetAsync();
        LoadForceEffects();

        Task.Run(async () =>
        {
            await CenterWheel();
        });
        
        while (true)
        {
            string message = messageServer.ReceiveFrameString();
            int type = (int)Char.GetNumericValue(message[0]);
            
            if (type == (int)MessageType.Rumble)
            {
                ActivateRumbleMessage rumbleMessage = JsonSerializer.Deserialize<ActivateRumbleMessage>(message.Substring(1));
                rumbleWheel.Stop();
                rumbleWheel.Gain = feedbackSettings.RumbleForce * Utils.Map(
                    (int)rumbleMessage.LargeMotor + (int)rumbleMessage.SmallMotor,
                    0, 510, 0, 1);
                rumbleWheel.Start();
            }

            if (type == (int)MessageType.Setting)
            {
                feedbackSettings = JsonSerializer.Deserialize<FeedbackSettings>(message.Substring(1));
                rumbleWheel.SetParameters(new(0.5f, 0, 0), feedbackSettings.RumbleFrequency, 0.5f, 0f, TimeSpan.MaxValue);
                turnWheelLeft.Gain = feedbackSettings.CenterSpringForce;
                turnWheelRight.Gain = feedbackSettings.CenterSpringForce;

            }
            
            messageServer.SendFrame("message received");
        }
    }

    async Task CenterWheel()
    {
        while (true)
        {
            if (racingWheel.GetCurrentReading().Wheel > feedbackSettings.CenterSpringDeadzone / 90)
            {
                if (turnWheelLeft.State != ForceFeedbackEffectState.Running)
                    turnWheelLeft.Start();
            }
            else
            {
                if (turnWheelLeft.State == ForceFeedbackEffectState.Running)
                    turnWheelLeft.Stop();
            }

            if (racingWheel.GetCurrentReading().Wheel < -(feedbackSettings.CenterSpringDeadzone / 90))
            {
                if (turnWheelRight.State != ForceFeedbackEffectState.Running)
                    turnWheelRight.Start();
            }
            else
            {
                if (turnWheelRight.State == ForceFeedbackEffectState.Running)
                    turnWheelRight.Stop();
            }
        }
    }

    async void LoadForceEffects()
    {
        turnWheelLeft = new();
        turnWheelRight = new();
        rumbleWheel = new(PeriodicForceEffectKind.TriangleWave);
        turnWheelLeft.SetParameters(new(0.5f, 0, 0), TimeSpan.MaxValue);
        turnWheelRight.SetParameters(new(-0.5f, 0, 0), TimeSpan.MaxValue);
        rumbleWheel.SetParameters(new(0.5f, 0, 0), feedbackSettings.RumbleFrequency, 0.5f, 0f, TimeSpan.MaxValue);
        
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
        racingWheel.WheelMotor.TryUnloadEffectAsync(rumbleWheel);
    }
}