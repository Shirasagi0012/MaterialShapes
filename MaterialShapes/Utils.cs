using Avalonia;

namespace MaterialShapes;

internal static class Utils
{
    public const double DistanceEpsilon = 1e-4;
    public const double AngleEpsilon = 1e-6;

    internal static bool Convex(Point previous, Point current, Point next)
    {
        return (current - previous).Clockwise(next - current);
    }

    internal static double Square(double v)
    {
        return v * v;
    }

    internal static double DistanceSquared(Vector v)
    {
        return v.X * v.X + v.Y * v.Y;
    }

    internal static double DistanceSquared(double dx, double dy)
    {
        return dx * dx + dy * dy;
    }

    internal static double Distance(Vector v)
    {
        return Math.Sqrt(DistanceSquared(v));
    }

    internal static double Distance(double dx, double dy)
    {
        return Math.Sqrt(DistanceSquared(dx, dy));
    }

    internal static Point RadialToCartesian(double radius, double angleRadians)
    {
        return new Point(radius * Math.Cos(angleRadians), radius * Math.Sin(angleRadians));
    }

    internal static Point Interpolate(Point p0, Point p1, double t)
    {
        return new Point(p0.X + (p1.X - p0.X) * t, p0.Y + (p1.Y - p0.Y) * t);
    }

    internal static Point DirectionVector(double dx, double dy)
    {
        return new Point(dx, dy).GetDirection();
    }

    internal static Point DirectionVector(double angleRadians)
    {
        return new Point(Math.Cos(angleRadians), Math.Sin(angleRadians));
    }

    internal static double DistanceSqurared(Point point)
    {
        return point.X * point.X + point.Y * point.Y;
    }

    internal static double DistanceSquared(Point point)
    {
        return point.X * point.X + point.Y * point.Y;
    }

    public static double PositiveModulo(double num, double mod)
    {
        if (mod <= 0)
            throw new ArgumentOutOfRangeException(nameof(mod));

        var m = num % mod;
        return m < 0 ? m + mod : m;
    }

    public static Rect UnionBounds(Rect a, Rect b)
    {
        var left = Math.Min(a.Left, b.Left);
        var top = Math.Min(a.Top, b.Top);
        var right = Math.Max(a.Right, b.Right);
        var bottom = Math.Max(a.Bottom, b.Bottom);
        return new Rect(left, top, right - left, bottom - top);
    }

    public static Point RadicalToCartesian(double radius, double angleRadians, Point center = default)
    {
        return DirectionVector(angleRadians) * radius + center;
    }
}