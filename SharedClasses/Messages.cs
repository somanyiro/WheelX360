namespace SharedClasses;

[Serializable]
public class ActivateRumbleMessage
{
    public ActivateRumbleMessage(byte smallMotor, byte largeMotor)
    {
        this.smallMotor = smallMotor;
        this.largeMotor = largeMotor;
    }
    
    public byte smallMotor;
    public byte largeMotor;
}

[Serializable]
public class FeedbackSettings
{
    public bool rumbleEnabled = true;
    public float rumbleForce = 0.5f;
    public bool centerSpringEnabled = true;
    public float centerSpringForce = 0.5f;
}