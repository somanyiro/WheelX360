using Windows.Gaming.Input;

namespace WheelX360;

public class ButtonMapping
{
    public RacingWheelButtons A { get; set; }
    public RacingWheelButtons B { get; set; }
    public RacingWheelButtons X { get; set; }
    public RacingWheelButtons Y { get; set; }
    public RacingWheelButtons Up { get; set; }
    public RacingWheelButtons Down { get; set; }
    public RacingWheelButtons Left { get; set; }
    public RacingWheelButtons Right { get; set; }
    public RacingWheelButtons Start { get; set; }
    public RacingWheelButtons Back { get; set; }
    public RacingWheelButtons LeftThumb { get; set; }
    public RacingWheelButtons RightThumb { get; set; }
    public RacingWheelButtons LeftShoulder { get; set; }
    public RacingWheelButtons RightShoulder { get; set; }
    public RacingWheelButtons Guide { get; set; }

    public RacingWheelAxis LeftThumbX { get; set; }
    public RacingWheelAxis LeftThumbY { get; set; }
    public RacingWheelAxis RightThumbX { get; set; }
    public RacingWheelAxis RightThumbY { get; set; }

    public RacingWheelAxis LeftTrigger { get; set; }
    public RacingWheelAxis RightTrigger { get; set; }

    public static short GetAxisValueShort(RacingWheelAxis axis, RacingWheelReading reading)
    {
        return axis switch
        {
            RacingWheelAxis.None => 0,
            RacingWheelAxis.Wheel => (short)Map((float)reading.Wheel, -1, 1, short.MinValue, short.MaxValue),
            RacingWheelAxis.Throttle => (short)Map((float)reading.Throttle, 0, 1, 0, short.MaxValue),
            RacingWheelAxis.Brake => (short)Map((float)reading.Brake, 0, 1, 0, short.MaxValue),
            RacingWheelAxis.Clutch => (short)Map((float)reading.Clutch, 0, 1, 0, short.MaxValue),
            RacingWheelAxis.Handbrake => (short)Map((float)reading.Handbrake, 0, 1, 0, short.MaxValue),
            _ => 0
        };
    }
    
    public static byte GetAxisValueByte(RacingWheelAxis axis, RacingWheelReading reading)
    {
        return axis switch
        {
            RacingWheelAxis.None => 0,
            RacingWheelAxis.Wheel => (byte)Map((float)reading.Wheel, -1, 1, 0, 255),
            RacingWheelAxis.Throttle => (byte)Map((float)reading.Throttle, 0, 1, 0, 255),
            RacingWheelAxis.Brake => (byte)Map((float)reading.Brake, 0, 1, 0, 255),
            RacingWheelAxis.Clutch => (byte)Map((float)reading.Clutch, 0, 1, 0, 255),
            RacingWheelAxis.Handbrake => (byte)Map((float)reading.Handbrake, 0, 1, 0, 255),
            _ => 0
        };
    }
    
    static float Map(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        value = Math.Clamp(value, oldMin, oldMax);
        return (value - oldMin) * (newMax - newMin) / (oldMax - oldMin) + newMin;
    }
}

public enum RacingWheelAxis { None, Wheel, Throttle, Brake, Clutch, Handbrake }

