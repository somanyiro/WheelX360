using System.Diagnostics;
using System.Reflection;

string currentAssemblyPath = Assembly.GetExecutingAssembly().Location;
string currentDirectory = Path.GetDirectoryName(currentAssemblyPath);

string forceFeedback = Path.Combine(currentDirectory, "WheelForceFeedback.exe");

ProcessStartInfo startInfo = new ProcessStartInfo(forceFeedback);
#if !DEBUG
startInfo.CreateNoWindow = true;
startInfo.UseShellExecute = false;
#endif
Process.Start(startInfo);

Console.WriteLine("Hello, World! 1");