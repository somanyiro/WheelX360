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
    
    private FeedbackSettings feedbackSettings = new FeedbackSettings();
    
    private Texture2D wheelTexture;
    private Texture2D controllerTexture;

    private ButtonMapping buttonMapping;
    
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

        //load button mappings
        try
        {
            string savedButtonMapping = File.ReadAllText("ButtonMapping.json");
            buttonMapping = JsonSerializer.Deserialize<ButtonMapping>(savedButtonMapping);
        }
        catch
        {
            CreateDefaultButtonMapping();
        }

        //create communication client for force feedback program
        using var messageClient = new RequestSocket(">tcp://localhost:5556");

        //tell the wheel to rumble if the controller rumbles
        controller.FeedbackReceived += (sender, args) =>
        {
            Console.WriteLine($"rumble: large:{args.LargeMotor} small:{args.SmallMotor}");
            var message = JsonSerializer.Serialize(new ActivateRumbleMessage
                { LargeMotor = args.LargeMotor, SmallMotor = args.SmallMotor });
            messageClient.SendFrame((int)MessageType.Rumble + message);
            messageClient.ReceiveFrameString();
        };
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            rlImGui.Begin();
            ImGui.Begin("window", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowSize(new Vector2(500, 600));
            ImGui.SetWindowPos(new Vector2(0,0));
            Raylib.ClearBackground(Color.DarkGray);
            //================================ main loop ================================

            if (!controllerConnected)
            {
                if (ImGui.Button("Connect"))
                {
                    controller.Connect();
                    controllerConnected = true;
                }
            }
            else
            {
                if (ImGui.Button("Disconnect"))
                {
                    controller.Disconnect();
                    controllerConnected = false;
                }
            }
            
            Raylib.DrawTexturePro(
                wheelTexture,
                new Rectangle(0, 0, wheelTexture.Width, wheelTexture.Height),
                new Rectangle(150, 300, 200, 200),
                new Vector2(100, 100),
                ((float)racingWheel.GetCurrentReading().Wheel * 90f),
                Color.White);
        
            Raylib.DrawTexturePro(
                controllerTexture,
                new Rectangle(0, 0, controllerTexture.Width, controllerTexture.Height),
                new Rectangle(350, 300, 200, 200),
                new Vector2(100, 100),
                0f,
                Color.White);

            if (ImGui.RadioButton("Enable centering force", feedbackSettings.CenterSpringEnabled))
                feedbackSettings.CenterSpringEnabled = !feedbackSettings.CenterSpringEnabled;
            float centerSpringForce = feedbackSettings.CenterSpringForce;
            ImGui.DragFloat("Centering power", ref centerSpringForce, 0.01f, 0f, 1f);
            feedbackSettings.CenterSpringForce = centerSpringForce;
            
            if (ImGui.RadioButton("Enable rumble", feedbackSettings.RumbleEnabled))
                feedbackSettings.RumbleEnabled = !feedbackSettings.RumbleEnabled;

            if (feedbackSettings.RumbleEnabled)
            {
                float rumbleForce = feedbackSettings.RumbleForce;
                ImGui.DragFloat("Rumble power", ref rumbleForce, 0f, 0f, 1f);
                feedbackSettings.RumbleForce = rumbleForce;

                float rumbleFrequency = feedbackSettings.RumbleFrequency;
                ImGui.DragFloat("Rumble frequency", ref rumbleFrequency, 0f, 4.5f, 12.5f);
                feedbackSettings.RumbleFrequency = rumbleFrequency;
            }
            
            
            if (ImGui.Button("Apply settings"))
            {
                var message = JsonSerializer.Serialize(feedbackSettings);
                messageClient.SendFrame((int)MessageType.Setting + message);
                messageClient.ReceiveFrameString();
            }

            if (ImGui.Button("Test motor"))
            {
                var message = JsonSerializer.Serialize(new ActivateRumbleMessage
                    { LargeMotor = 255, SmallMotor = 255 });
                messageClient.SendFrame((int)MessageType.Rumble + message);
                messageClient.ReceiveFrameString();
            }
        
            if (controllerConnected)
                UpdateControllerState();
            
            //================================ cleanup ================================
            rlImGui.End();
            Raylib.EndDrawing();
        }
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
            Start = RacingWheelButtons.Button7,
            Back = RacingWheelButtons.Button8,
            LeftThumb = RacingWheelButtons.None,
            RightThumb = RacingWheelButtons.None,
            LeftShoulder = RacingWheelButtons.Button6,
            RightShoulder = RacingWheelButtons.Button5,
            Guide = RacingWheelButtons.Button11,
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
    
}