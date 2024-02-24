using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.Gaming.Input.ForceFeedback;
using H.Pipes;
using SharedClasses;

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
        CreateNamedPipeServers();
        //LoadForceEffects();
        
    }

    async void CreateNamedPipeServers()
    {
        await using var rumblePipeServer = new PipeServer<ActivateRumbleMessage>("RumbleMessagePipe");
        await using var settingsPipeServer = new PipeServer<FeedbackSettings>("FeedbackSettingsPipe");

        rumblePipeServer.MessageReceived += (sender, args) =>
        {
            Console.WriteLine(
                $"rumble should be activated with {(int)args.Message.largeMotor} {(int)args.Message.smallMotor}");
        };

        settingsPipeServer.MessageReceived += (sender, args) =>
        {
            Console.WriteLine("new settings received");
        };
        
        await rumblePipeServer.StartAsync();
        await settingsPipeServer.StartAsync();
        
        Console.WriteLine("started servers");
        
        await Task.Delay(Timeout.InfiniteTimeSpan);
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