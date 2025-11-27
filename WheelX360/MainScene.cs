using System.Numerics;
using System.Text.Json;
using Windows.Gaming.Input;
using ImGuiNET;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using NetMQ;
using NetMQ.Sockets;
using Raylib_cs;
using rlImGui_cs;
using SharedClasses;

namespace WheelX360;

public class MainScene
{
    private ViGEmClient inputClient;
    private IXbox360Controller controller;
    private bool controllerConnected = false;
    RacingWheel racingWheel;
    
    private FeedbackSettings feedbackSettings;
    private ButtonMapping buttonMapping;
    private UITexts UILocalization;
    
    private Texture2D wheelTexture;
    private Texture2D controllerTexture;

    
    public MainScene()
    {
        inputClient = new ViGEmClient();
    }

    ~MainScene()
    {
    }

    public void Run()
    {
        //================================ setup ================================
        //create controller and access wheel
        controller = inputClient.CreateXbox360Controller();
        racingWheel = RacingWheel.RacingWheels[0];
        
        //load resources
        wheelTexture = Raylib.LoadTexture("resources/g920.png");
        controllerTexture = Raylib.LoadTexture("resources/xbox360.png");

        //create communication client for force feedback program
        using var messageClient = new RequestSocket(">tcp://localhost:5556"); //TODO: make port configurable

        //tell the wheel to rumble if the controller rumbles
        controller.FeedbackReceived += (sender, args) =>
        {
            Console.WriteLine($"rumble: large:{args.LargeMotor} small:{args.SmallMotor}");
            var message = JsonSerializer.Serialize(new ActivateRumbleMessage
                { LargeMotor = args.LargeMotor, SmallMotor = args.SmallMotor });
            messageClient.SendFrame((int)MessageType.Rumble + message);
            messageClient.ReceiveFrameString();
        };
        
        //TODO: implement choosing between languages in UI
        //load UI localization
        try
        {
            string localizationFile = File.ReadAllText("Localization_en.json");
            UILocalization = JsonSerializer.Deserialize<UITexts>(localizationFile);
        }
        catch
        {
            Console.WriteLine("no localization file found");
            UILocalization = new UITexts();
            string json = JsonSerializer.Serialize(UILocalization, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("Localization_en.json", json);
            throw;
        }
        
        //load button mappings
        try
        {
            string savedButtonMapping = File.ReadAllText("ButtonMapping.json");
            buttonMapping = JsonSerializer.Deserialize<ButtonMapping>(savedButtonMapping);
        }
        catch
        {
            Console.WriteLine("no button mapping found");
            CreateDefaultButtonMapping();
        }

        //laod feedback settings
        try
        {
            string savedFeedbackSettings = File.ReadAllText("FeedbackSettings.json");
            feedbackSettings = JsonSerializer.Deserialize<FeedbackSettings>(savedFeedbackSettings);
            ApplySettigns(messageClient);
        }
        catch
        {
            Console.WriteLine("no feedback settings found");
            feedbackSettings = new();
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(feedbackSettings, jsonOptions);
            File.WriteAllText("FeedbackSettings.json", json);
        }
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            rlImGui.Begin();
            ImGui.Begin(UILocalization.ProgramTitle, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowSize(new Vector2(500, 600));
            ImGui.SetWindowPos(new Vector2(0,0));
            Raylib.ClearBackground(Color.DarkGray);
            //================================ main loop ================================

            if (!controllerConnected)
            {
                if (ImGui.Button(UILocalization.ControllerConnectButton))
                {
                    controller.Connect();
                    controllerConnected = true;
                }
            }
            else
            {
                if (ImGui.Button(UILocalization.ControllerDisconnectButton))
                {
                    controller.Disconnect();
                    controllerConnected = false;
                }
            }
            
            Raylib.DrawTexturePro(
                wheelTexture,
                new Rectangle(0, 0, wheelTexture.Width, wheelTexture.Height),
                new Rectangle(250, 300, 400, 400),
                new Vector2(200, 200),
                ((float)racingWheel.GetCurrentReading().Wheel * 90f),
                Color.White);

            if (ImGui.RadioButton(UILocalization.CenteringForceRadioButton, feedbackSettings.CenterSpringEnabled))
                feedbackSettings.CenterSpringEnabled = !feedbackSettings.CenterSpringEnabled;

            if (feedbackSettings.CenterSpringEnabled)
            {
                float centerSpringForce = feedbackSettings.CenterSpringForce;
                ImGui.DragFloat(UILocalization.CenteringForceStrengthInput, ref centerSpringForce, 0f, 0f, 1f);
                feedbackSettings.CenterSpringForce = centerSpringForce;

                float centerSpringDeadzone = feedbackSettings.CenterSpringDeadzone;
                ImGui.DragFloat(UILocalization.CenteringForceDeadzoneInput, ref centerSpringDeadzone, 0f, 0f, 5f);
                feedbackSettings.CenterSpringDeadzone = centerSpringDeadzone;
            }
            
            if (ImGui.RadioButton(UILocalization.EnableRumbleRadioButton, feedbackSettings.RumbleEnabled))
                feedbackSettings.RumbleEnabled = !feedbackSettings.RumbleEnabled;

            if (feedbackSettings.RumbleEnabled)
            {
                float rumbleForce = feedbackSettings.RumbleForce;
                ImGui.DragFloat(UILocalization.RumbleStrengthInput, ref rumbleForce, 0f, 0f, 1f);
                feedbackSettings.RumbleForce = rumbleForce;

                float rumbleFrequency = feedbackSettings.RumbleFrequency;
                ImGui.DragFloat(UILocalization.RumbleFrequencyInput, ref rumbleFrequency, 0f, 4.5f, 12.5f);
                feedbackSettings.RumbleFrequency = rumbleFrequency;
            }
            
            if (ImGui.Button(UILocalization.ApplySettingsButton))
            {
                ApplySettigns(messageClient);
                
                //save settings
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(feedbackSettings, jsonOptions);
                File.WriteAllText("FeedbackSettings.json", json);
            }

            if (ImGui.Button(UILocalization.TestMotorButton))
            {
                var message = JsonSerializer.Serialize(new ActivateRumbleMessage
                    { LargeMotor = 255, SmallMotor = 255 });
                messageClient.SendFrame((int)MessageType.Rumble + message);
                messageClient.ReceiveFrameString();
            }

            if (ImGui.Button(UILocalization.ReloadEffectsButton) || (racingWheel.GetCurrentReading().Buttons & RacingWheelButtons.Button6) != 0)
            {
                messageClient.SendFrame(((int)MessageType.Reload).ToString());
                messageClient.ReceiveFrameString();
            }
            
            if (ImGui.Button(UILocalization.ResetWheelButton))
            {
                messageClient.SendFrame(((int)MessageType.Stop).ToString());
                messageClient.ReceiveFrameString();
            }
            
            if (controllerConnected)
                UpdateControllerState();

            DisplayInputReading();
            
            //================================ cleanup ================================
            rlImGui.End();
            Raylib.EndDrawing();
        }
    }

    void ApplySettigns(RequestSocket socket)
    {
        var message = JsonSerializer.Serialize(feedbackSettings);
        socket.SendFrame((int)MessageType.Setting + message);
        socket.ReceiveFrameString();
    }
    
    void UpdateControllerState()
    {
        RacingWheelReading reading = racingWheel.GetCurrentReading();
        controller.SetButtonState(Xbox360Button.A, (reading.Buttons & buttonMapping.A) != 0);
        controller.SetButtonState(Xbox360Button.B, (reading.Buttons & buttonMapping.B) != 0);
        controller.SetButtonState(Xbox360Button.X, (reading.Buttons & buttonMapping.X) != 0);
        controller.SetButtonState(Xbox360Button.Y, (reading.Buttons & buttonMapping.Y) != 0);
        controller.SetButtonState(Xbox360Button.Up, (reading.Buttons & buttonMapping.Up) != 0);
        controller.SetButtonState(Xbox360Button.Down, (reading.Buttons & buttonMapping.Down) != 0);
        controller.SetButtonState(Xbox360Button.Left, (reading.Buttons & buttonMapping.Left) != 0);
        controller.SetButtonState(Xbox360Button.Right, (reading.Buttons & buttonMapping.Right) != 0);
        controller.SetButtonState(Xbox360Button.Start, (reading.Buttons & buttonMapping.Start) != 0);
        controller.SetButtonState(Xbox360Button.Back, (reading.Buttons & buttonMapping.Back) != 0);
        controller.SetButtonState(Xbox360Button.LeftThumb, (reading.Buttons & buttonMapping.LeftThumb) != 0);
        controller.SetButtonState(Xbox360Button.RightThumb, (reading.Buttons & buttonMapping.RightThumb) != 0);
        controller.SetButtonState(Xbox360Button.LeftShoulder, (reading.Buttons & buttonMapping.LeftShoulder) != 0);
        controller.SetButtonState(Xbox360Button.RightShoulder, (reading.Buttons & buttonMapping.RightShoulder) != 0);
        controller.SetButtonState(Xbox360Button.Guide, (reading.Buttons & buttonMapping.Guide) != 0);
        
        controller.SetAxisValue(Xbox360Axis.LeftThumbX, ButtonMapping.GetAxisValueShort(buttonMapping.LeftThumbX, reading));
        controller.SetAxisValue(Xbox360Axis.LeftThumbY, ButtonMapping.GetAxisValueShort(buttonMapping.LeftThumbY, reading));
        controller.SetAxisValue(Xbox360Axis.RightThumbX, ButtonMapping.GetAxisValueShort(buttonMapping.RightThumbX, reading));
        controller.SetAxisValue(Xbox360Axis.RightThumbY, ButtonMapping.GetAxisValueShort(buttonMapping.RightThumbY, reading));
        
        controller.SetSliderValue(Xbox360Slider.LeftTrigger, ButtonMapping.GetAxisValueByte(buttonMapping.LeftTrigger, reading));
        controller.SetSliderValue(Xbox360Slider.RightTrigger, ButtonMapping.GetAxisValueByte(buttonMapping.RightTrigger, reading));
    }

    void CreateDefaultButtonMapping()
    {
        buttonMapping = new ButtonMapping // my button mappings for the g920
        {
            A = RacingWheelButtons.Button1,
            B = RacingWheelButtons.Button2,
            X = RacingWheelButtons.Button3,
            Y = RacingWheelButtons.Button4,
            Up = RacingWheelButtons.DPadUp,
            Down = RacingWheelButtons.DPadDown,
            Left = RacingWheelButtons.DPadLeft,
            Right = RacingWheelButtons.DPadRight,
            Start = RacingWheelButtons.Button8,
            Back = RacingWheelButtons.Button7,
            LeftThumb = RacingWheelButtons.None,
            RightThumb = RacingWheelButtons.None,
            LeftShoulder = RacingWheelButtons.PreviousGear,
            RightShoulder = RacingWheelButtons.NextGear,
            Guide = RacingWheelButtons.None,
            LeftThumbX = RacingWheelAxis.Wheel,
            LeftThumbY = RacingWheelAxis.None,
            RightThumbX = RacingWheelAxis.None,
            RightThumbY = RacingWheelAxis.None,
            LeftTrigger = RacingWheelAxis.Clutch,
            RightTrigger = RacingWheelAxis.Throttle
        };
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(buttonMapping, jsonOptions);
        File.WriteAllText("ButtonMapping.json", json);
    }

    void DisplayInputReading()
    {
        var r = racingWheel.GetCurrentReading();
        var b = r.Buttons;
        if ((b & RacingWheelButtons.Button1) != 0) ImGui.Text($"{RacingWheelButtons.Button1}");
        if ((b & RacingWheelButtons.Button2) != 0) ImGui.Text($"{RacingWheelButtons.Button2}");
        if ((b & RacingWheelButtons.Button3) != 0) ImGui.Text($"{RacingWheelButtons.Button3}");
        if ((b & RacingWheelButtons.Button4) != 0) ImGui.Text($"{RacingWheelButtons.Button4}");
        if ((b & RacingWheelButtons.Button5) != 0) ImGui.Text($"{RacingWheelButtons.Button5}");
        if ((b & RacingWheelButtons.Button6) != 0) ImGui.Text($"{RacingWheelButtons.Button6}");
        if ((b & RacingWheelButtons.Button7) != 0) ImGui.Text($"{RacingWheelButtons.Button7}");
        if ((b & RacingWheelButtons.Button8) != 0) ImGui.Text($"{RacingWheelButtons.Button8}");
        if ((b & RacingWheelButtons.Button9) != 0) ImGui.Text($"{RacingWheelButtons.Button9}");
        if ((b & RacingWheelButtons.Button10) != 0) ImGui.Text($"{RacingWheelButtons.Button10}");
        if ((b & RacingWheelButtons.Button11) != 0) ImGui.Text($"{RacingWheelButtons.Button11}");
        if ((b & RacingWheelButtons.Button12) != 0) ImGui.Text($"{RacingWheelButtons.Button12}");
        if ((b & RacingWheelButtons.Button13) != 0) ImGui.Text($"{RacingWheelButtons.Button13}");
        if ((b & RacingWheelButtons.Button14) != 0) ImGui.Text($"{RacingWheelButtons.Button14}");
        if ((b & RacingWheelButtons.Button15) != 0) ImGui.Text($"{RacingWheelButtons.Button15}");
        if ((b & RacingWheelButtons.Button16) != 0) ImGui.Text($"{RacingWheelButtons.Button16}");
        if ((b & RacingWheelButtons.NextGear) != 0) ImGui.Text($"{RacingWheelButtons.NextGear}");
        if ((b & RacingWheelButtons.PreviousGear) != 0) ImGui.Text($"{RacingWheelButtons.PreviousGear}");
        if ((b & RacingWheelButtons.DPadUp) != 0) ImGui.Text($"{RacingWheelButtons.DPadUp}");
        if ((b & RacingWheelButtons.DPadRight) != 0) ImGui.Text($"{RacingWheelButtons.DPadRight}");
        if ((b & RacingWheelButtons.DPadDown) != 0) ImGui.Text($"{RacingWheelButtons.DPadDown}");
        if ((b & RacingWheelButtons.DPadLeft) != 0) ImGui.Text($"{RacingWheelButtons.DPadLeft}");

        if (r.Throttle != 0) ImGui.Text($"Throttle: {r.Throttle}");
        if (r.Brake != 0) ImGui.Text($"Brake: {r.Brake}");
        if (r.Clutch != 0) ImGui.Text($"Clutch: {r.Clutch}");
        if (r.Wheel != 0) ImGui.Text($"Wheel: {r.Wheel}");
        if (r.Handbrake != 0) ImGui.Text($"Handbrake: {r.Handbrake}");
        if (r.PatternShifterGear != 0) ImGui.Text($"PatternShifterGear: {r.PatternShifterGear}");
    }
    
}