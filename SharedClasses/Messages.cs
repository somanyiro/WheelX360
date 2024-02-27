namespace SharedClasses;

public enum MessageType
{
    Rumble,
    Setting,
    Reload,
    Stop
}

public class ActivateRumbleMessage
{
    public byte SmallMotor { get; set; }
    public byte LargeMotor { get; set; }
}

public class FeedbackSettings
{
    public bool RumbleEnabled { get; set; } = true;
    public float RumbleForce { get; set; } = 0.5f;
    public float RumbleFrequency { get; set; } = 8.5f;
    public bool CenterSpringEnabled { get; set; } = true;
    public float CenterSpringForce { get; set; } = 0.5f;
    public float CenterSpringDeadzone { get; set; } = 1f;
}