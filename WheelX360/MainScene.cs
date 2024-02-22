using System.Numerics;
using System.Text.Json;
using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.Gaming.Input.ForceFeedback;
using ImGuiNET;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Raylib_cs;
using rlImGui_cs;

namespace WheelX360;

public class MainScene
{
    private ViGEmClient inputClient = new ViGEmClient();
    private IXbox360Controller controller;
    private bool controllerConnected = false;
    RacingWheel racingWheel;

    private ConstantForceEffect turnWheelLeft = new ConstantForceEffect();
    private ConstantForceEffect turnWheelRight = new ConstantForceEffect();
    private PeriodicForceEffect rumbleWheel = new PeriodicForceEffect(PeriodicForceEffectKind.SineWave);
    
    private bool centerSpringEnabled = false;
    private static float centerSpringForce = 0.5f;
    private bool rumbleEnabled = false;
    private static float rumbleForce = 0.5f;

    private Texture2D wheelTexture;
    private Texture2D controllerTexture;

    private ButtonMapping buttonMapping;
    
    public MainScene()
    {
    }

    ~MainScene()
    {
    }

    public void Run()
    {
        
        //================================ setup ================================
        controller = inputClient.CreateXbox360Controller();
        racingWheel = RacingWheel.RacingWheels[0];
        
        //ExecFunction.RunAsync(() => ForceFeedback());
        /*
        Thread forceFeedback = new Thread(() => ForceFeedback());
        forceFeedback.Start();
        */
        //LoadForceEffects(racingWheel);
        
        wheelTexture = Raylib.LoadTexture("resources/g920.png");
        controllerTexture = Raylib.LoadTexture("resources/xbox360.png");

        string savedButtonMapping = File.ReadAllText("ButtonMapping.json");
        buttonMapping = JsonSerializer.Deserialize<ButtonMapping>(savedButtonMapping);
        if (buttonMapping is null)
            CreateDefaultButtonMapping();
        
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

            if (ImGui.RadioButton("Enable centering force", centerSpringEnabled))
                centerSpringEnabled = !centerSpringEnabled;
            ImGui.DragFloat("Centering power", ref centerSpringForce, 0.01f, 0f, 1f);
            
            if (ImGui.RadioButton("Enable rumble", rumbleEnabled))
                rumbleEnabled = !rumbleEnabled;
            ImGui.DragFloat("Rumble power", ref rumbleForce, 0f, 0f, 1f);
        
            if (ImGui.Button("Apply settings"))
            {
                UnLoadForceEffects();
                LoadForceEffects();
            }

            if (ImGui.Button("Test motor"))
            {
                turnWheelRight.Start();
            }
        
            if (controllerConnected)
                UpdateControllerState();
            
            //================================ cleanup ================================
            rlImGui.End();
            Raylib.EndDrawing();
        }
        
        UnLoadForceEffects();
    }

    void UnLoadForceEffects()
    {
        racingWheel.WheelMotor.TryUnloadEffectAsync(turnWheelLeft);
        racingWheel.WheelMotor.TryUnloadEffectAsync(turnWheelRight);
        racingWheel.WheelMotor.TryUnloadEffectAsync(rumbleWheel);
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