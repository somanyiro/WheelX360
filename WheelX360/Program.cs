using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Windows.Gaming.Input;
using ImGuiNET;
using Nefarius.Drivers.HidHide;
using Raylib_cs;
using rlImGui_cs;

namespace WheelX360;

public static class Program
{
    public static void Main(string[] args)
    {
        StartForceFeedback();
        
        Raylib.SetTraceLogLevel(TraceLogLevel.Fatal);
        Raylib.InitWindow(500, 600, "WheelX360");
        rlImGui.Setup(true, true);
        
        FindWheel();
        HidHideControlService hidHideControlService = new();
        Console.WriteLine(hidHideControlService.IsInstalled ? "hidhide installed" : "no hidhide found");
        new MainScene().Run();

        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }

    static void FindWheel()
    {
        while (!Raylib.WindowShouldClose() && RacingWheel.RacingWheels.Count == 0)
        {
            Raylib.BeginDrawing();
            rlImGui.Begin();
            ImGui.Begin("window", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowSize(new Vector2(500, 600));
            ImGui.SetWindowPos(new Vector2(0,0));
            Raylib.ClearBackground(Color.DarkGray);
            
            ImGui.Text("Searching for racing wheel");
            ImGui.Text("\nIf this takes more than a second,\nconsider starting the software of your wheel (GHUB)");
            
            rlImGui.End();
            Raylib.EndDrawing();
        }
    }

    static void StartForceFeedback()
    { 
        //so I don't know why, but the force feedback has to be in a separate app to work
        //I know what you think, but multi threading or even running it in a separate process doesn't fix it
        //the moment InitWindow is ran, the force feedback just stops unless I have it in a separate app :/
        string currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
        string currentDirectory = Path.GetDirectoryName(currentAssemblyPath);

        string forceFeedback = Path.Combine(currentDirectory, "WheelForceFeedback.exe");

        ProcessStartInfo startInfo = new ProcessStartInfo(forceFeedback);
        #if !DEBUG
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        #endif
        Process.Start(startInfo);
    }
}
