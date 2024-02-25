namespace SharedClasses;

public static class Utils
{
    public static float Map(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        value = Math.Clamp(value, oldMin, oldMax);
        return (value - oldMin) * (newMax - newMin) / (oldMax - oldMin) + newMin;
    }
}