using Windows.Gaming.Input;
using SharedClasses;

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
            RacingWheelAxis.Wheel => (short)Utils.Map((float)reading.Wheel, -1, 1, short.MinValue, short.MaxValue),
            RacingWheelAxis.Throttle => (short)Utils.Map((float)reading.Throttle, 0, 1, 0, short.MaxValue),
            RacingWheelAxis.Brake => (short)Utils.Map((float)reading.Brake, 0, 1, 0, short.MaxValue),
            RacingWheelAxis.Clutch => (short)Utils.Map((float)reading.Clutch, 0, 1, 0, short.MaxValue),
            RacingWheelAxis.Handbrake => (short)Utils.Map((float)reading.Handbrake, 0, 1, 0, short.MaxValue),
            _ => 0
        };
    }
    
    public static byte GetAxisValueByte(RacingWheelAxis axis, RacingWheelReading reading)
    {
        return axis switch
        {
            RacingWheelAxis.None => 0,
            RacingWheelAxis.Wheel => (byte)Utils.Map((float)reading.Wheel, -1, 1, 0, 255),
            RacingWheelAxis.Throttle => (byte)Utils.Map((float)reading.Throttle, 0, 1, 0, 255),
            RacingWheelAxis.Brake => (byte)Utils.Map((float)reading.Brake, 0, 1, 0, 255),
            RacingWheelAxis.Clutch => (byte)Utils.Map((float)reading.Clutch, 0, 1, 0, 255),
            RacingWheelAxis.Handbrake => (byte)Utils.Map((float)reading.Handbrake, 0, 1, 0, 255),
            _ => 0
        };
    }
}

public enum RacingWheelAxis { None, Wheel, Throttle, Brake, Clutch, Handbrake }

