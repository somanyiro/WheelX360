namespace SharedClasses;

[Serializable]
public class ActivateRumbleMessage
{
    public byte SmallMotor { get; set; }
    public byte LargeMotor { get; set; }
}

[Serializable]
public class FeedbackSettings
{
    public bool RumbleEnabled { get; set; }
    public float RumbleForce { get; set; }
    public bool CenterSpringEnabled { get; set; }
    public float centerSpringForce { get; set; }
}