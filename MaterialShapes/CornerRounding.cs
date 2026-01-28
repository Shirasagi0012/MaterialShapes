namespace MaterialShapes;

public readonly struct CornerRounding(double radius = 0, double smoothing = 0)
{
    public readonly double Radius = radius;
    public readonly double Smoothing = smoothing;

    public static readonly CornerRounding Unrounded = new();
}