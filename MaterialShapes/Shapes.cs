
using Avalonia;

namespace MaterialShapes;

public partial class RoundedPolygon
{
    public static RoundedPolygon CreateCircle (
        int numVertices = 8,
        double radius = 1D,
        Point center = default)
    {
        if ( numVertices < 3 )
            throw new ArgumentException("Circle must have at least three vertices");

        // Half of the angle between two adjacent vertices on the polygon
        var theta = Math.PI / numVertices;
        // Radius of the underlying RoundedPolygon object given the desired radius of the circle
        var polygonRadius = radius / Math.Cos(theta);

        return new RoundedPolygon(
            numVertices,
            polygonRadius,
            center,
            new CornerRounding(radius)
        );
    }

    public static RoundedPolygon CreateRectangle (
        double width = 2D,
        double height = 2D,
        CornerRounding? rounding = null,
        List<CornerRounding>? perVertexRounding = null,
        Point center = default
    )
    {
        var halfWidth = width / 2D;
        var halfHeight = height / 2D;

        var topLeft = new Point(center.X - halfWidth, center.Y - halfHeight);
        var topRight = new Point(center.X + halfWidth, center.Y - halfHeight);
        var bottomRight = new Point(center.X + halfWidth, center.Y + halfHeight);
        var bottomLeft = new Point(center.X - halfWidth, center.Y + halfHeight);

        return new RoundedPolygon(
            [bottomRight, bottomLeft, topLeft, topRight],
            rounding ?? CornerRounding.Unrounded,
            perVertexRounding,
            center
        );
    }

    public static RoundedPolygon CreateStar (
        int numVerticesPerRadius,
        double radius = 1D,
        double innerRadius = 0.5D,
        CornerRounding? rounding = null,
        CornerRounding? innerRounding = null,
        List<CornerRounding>? perVertexRounding = null,
        Point center = default
    )
    {
        if ( radius <= 0d || innerRadius <= 0d )
        {
            throw new ArgumentException("Star radii must both be greater than 0");
        }

        if ( innerRadius >= radius )
        {
            throw new ArgumentException("innerRadius must be less than radius");
        }

        var pvRounding = perVertexRounding;
        var roundingValue = rounding ?? CornerRounding.Unrounded;

        if ( pvRounding is null && innerRounding is not null )
        {
            pvRounding = Enumerable.Range(0, numVerticesPerRadius)
                .SelectMany(_ => new[ ] { roundingValue, innerRounding.Value })
                .ToList();
        }

        return new RoundedPolygon(
            StarVerticesFromNumVerts(numVerticesPerRadius, radius, innerRadius, center),
            roundingValue,
            pvRounding,
            center
        );
    }

    private static Point[ ] StarVerticesFromNumVerts (int numVerticesPerRadius, double radius, double innerRadius, Point center)
    {
        var result = new Point[numVerticesPerRadius * 2];
        var arrayIndex = 0;
        for ( var i = 0; i < numVerticesPerRadius; i++ )
        {
            var vertex = Utils.RadialToCartesian(radius, Math.PI / numVerticesPerRadius * 2 * i);
            result[arrayIndex++] = new Point(center.X + vertex.X, center.Y + vertex.Y);

            vertex = Utils.RadialToCartesian(innerRadius, Math.PI / numVerticesPerRadius * (2 * i + 1));
            result[arrayIndex++] = new Point(center.X + vertex.X, center.Y + vertex.Y);
        }

        return result;
    }

    public static RoundedPolygon CreatePill (
        double width = 2D,
        double height = 1D,
        double smoothing = 0D,
        Point center = default)
    {
        if ( width <= 0d || height <= 0d )
            throw new ArgumentException("Pill shapes must have positive width and height");

        var wHalf = width / 2d;
        var hHalf = height / 2d;

        var vertices = new[]
        {
            new Point(wHalf + center.X, hHalf + center.Y),
            new Point(-wHalf + center.X, hHalf + center.Y),
            new Point(-wHalf + center.X, -hHalf + center.Y),
            new Point(wHalf + center.X, -hHalf + center.Y),
        };

        return new RoundedPolygon(
            vertices,
            new CornerRounding(Math.Min(wHalf, hHalf), smoothing),
            perVertexRounding: null,
            center: center);
    }

    public static RoundedPolygon CreatePillStar (
        double width = 2D,
        double height = 1D,
        int numVerticesPerRadius = 8,
        double innerRadiusRatio = 0.5,
        CornerRounding? rounding = null,
        CornerRounding? innerRounding = null,
        List<CornerRounding>? perVertexRounding = null,
        double vertexSpacing = 0.5,
        double startLocation = 0.0,
        Point center = default)
    {
        if ( width <= 0d || height <= 0d )
            throw new ArgumentException("Pill shapes must have positive width and height");

        if ( innerRadiusRatio <= 0d || innerRadiusRatio > 1d )
            throw new ArgumentException("innerRadius must be between 0 and 1");

        var pvRounding = perVertexRounding;
        var roundingValue = rounding ?? CornerRounding.Unrounded;

        if ( pvRounding is null && innerRounding is not null )
        {
            pvRounding = Enumerable.Range(0, numVerticesPerRadius)
                .SelectMany(_ => new[ ] { roundingValue, innerRounding.Value })
                .ToList();
        }

        return new RoundedPolygon(
            PillStarVerticesFromNumVerts(
                numVerticesPerRadius,
                width,
                height,
                innerRadiusRatio,
                vertexSpacing,
                startLocation,
                center),
            roundingValue,
            pvRounding,
            center);
    }

    private static Point[ ] PillStarVerticesFromNumVerts (
        int numVerticesPerRadius,
        double width,
        double height,
        double innerRadiusRatio,
        double vertexSpacing,
        double startLocation,
        Point center)
    {
        // Width/height define the overall pill's bounding box; each endcap is a semicircle whose
        // radius is half of the smaller dimension.
        var endcapRadius = Math.Min(width, height) / 2d;

        var vSegLen = Math.Max(0d, height - width);
        var hSegLen = Math.Max(0d, width - height);
        var vSegHalf = vSegLen / 2d;
        var hSegHalf = hSegLen / 2d;

        var circlePerimeter = (2d * Math.PI) * endcapRadius * Interpolate(innerRadiusRatio, 1d, vertexSpacing);
        var perimeter = 2d * hSegLen + 2d * vSegLen + circlePerimeter;

        var sections = new double[11];
        sections[0] = 0d;
        sections[1] = vSegLen / 2d;
        sections[2] = sections[1] + circlePerimeter / 4d;
        sections[3] = sections[2] + hSegLen;
        sections[4] = sections[3] + circlePerimeter / 4d;
        sections[5] = sections[4] + vSegLen;
        sections[6] = sections[5] + circlePerimeter / 4d;
        sections[7] = sections[6] + hSegLen;
        sections[8] = sections[7] + circlePerimeter / 4d;
        sections[9] = sections[8] + vSegLen / 2d;
        sections[10] = perimeter;

        var tPerVertex = perimeter / (2d * numVerticesPerRadius);

        var inner = false;
        var currSecIndex = 0;
        var secStart = 0d;
        var secEnd = sections[1];

        var t = startLocation * perimeter;

        var result = new Point[numVerticesPerRadius * 2];
        var vertexIndex = 0;

        var rectBR = new Point(hSegHalf, vSegHalf);
        var rectBL = new Point(-hSegHalf, vSegHalf);
        var rectTL = new Point(-hSegHalf, -vSegHalf);
        var rectTR = new Point(hSegHalf, -vSegHalf);

        for ( var i = 0; i < numVerticesPerRadius * 2; i++ )
        {
            var boundedT = t % perimeter;
            //TODO: Check Correctness
            if ( boundedT < secStart )
            {
                currSecIndex = 0;
                secStart = sections[0];
                secEnd = sections[1];
            }

            while ( boundedT >= sections[(currSecIndex + 1) % sections.Length] )
            {
                currSecIndex = (currSecIndex + 1) % sections.Length;
                secStart = sections[currSecIndex];
                secEnd = sections[(currSecIndex + 1) % sections.Length];
            }

            var tInSection = boundedT - secStart;
            var tProportion = (secEnd - secStart) <= Utils.DistanceEpsilon ? 0d : tInSection / (secEnd - secStart);

            var currRadius = inner ? endcapRadius * innerRadiusRatio : endcapRadius;

            Point vertex = currSecIndex switch
            {
                0 => new Point(currRadius, tProportion * vSegHalf),
                1 => Utils.RadialToCartesian(currRadius, tProportion * (Math.PI / 2d)) + rectBR,
                2 => new Point(hSegHalf - tProportion * hSegLen, currRadius),
                3 => Utils.RadialToCartesian(currRadius, (Math.PI / 2d) + tProportion * (Math.PI / 2d)) + rectBL,
                4 => new Point(-currRadius, vSegHalf - tProportion * vSegLen),
                5 => Utils.RadialToCartesian(currRadius, Math.PI + tProportion * (Math.PI / 2d)) + rectTL,
                6 => new Point(-hSegHalf + tProportion * hSegLen, -currRadius),
                7 => Utils.RadialToCartesian(currRadius, (Math.PI * 1.5d) + tProportion * (Math.PI / 2d)) + rectTR,
                _ => new Point(currRadius, -vSegHalf + tProportion * vSegHalf),
            };

            result[vertexIndex++] = new Point(vertex.X + center.X, vertex.Y + center.Y);

            t += tPerVertex;
            inner = !inner;
        }

        return result;
    }

    private static double Interpolate (double from, double to, double t) => from + (to - from) * t;
}
