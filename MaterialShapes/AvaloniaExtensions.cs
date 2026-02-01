using Avalonia;
using Avalonia.Media;

namespace MaterialShapes;

public static class AvaloniaExtensions
{
    extension(StreamGeometryContext ctx)
    {
        /// <summary>
        /// If the RoundedPolygon contains Cubic Béziers, it will draw by creating a figure from the StreamGeometryContext. 
        /// The figure drawn is closed and filled. 
        /// </summary>
        /// <param name="polygon">The rounded polygon to draw.</param>
        public void DrawRoundedPolygon(RoundedPolygon polygon)
        {
            if (polygon.Cubics.Count == 0)
                return;
            var first = polygon.Cubics[0];
            ctx.BeginFigure(first.Anchor0, true);
            foreach (var cubic in polygon.Cubics)
                ctx.CubicBezierTo(cubic.Control0, cubic.Control1, cubic.Anchor1);
            ctx.EndFigure(true);
        }

        public void DrawMorph(Morph morph, double progress)
        {
            if (morph.MorphMatch.Count == 0)
                return;
            var first = morph.EnumerateCubics(progress).First();
            ctx.BeginFigure(first.Anchor0, true);
            foreach (var cubic in morph.EnumerateCubics(progress))
                ctx.CubicBezierTo(cubic.Control0, cubic.Control1, cubic.Anchor1);
            ctx.EndFigure(true);
        }
    }

    extension(RoundedPolygon polygon)
    {
        /// <summary>
        /// Converts a normalized RoundedPolygon to a StreamGeometry, scaling it to the specified size and rotating it by the specified degree.
        /// Note that the RoundedPolygon is expected to be normalized, by calling Normalized() method. 
        /// RoundedPolygon created with MaterialShape class are already normalized.
        /// </summary>
        public StreamGeometry ToGeometry(Size size, double rotationDegree = 0, bool stretch = false)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.DrawRoundedPolygon(polygon);
            }

            if (((size.Width <= 0 || size.Height <= 0) && rotationDegree == 0) || polygon.Cubics.Count == 0)
                return geometry;

            var scale = Math.Min(size.Width, size.Height);
            //var translate = Math.Abs(size.Width - size.Height) / 2;

            geometry.Transform = CreateTransformMatrix(
                stretch ? size.Width : scale,
                stretch ? size.Height : scale,
                //(size.Width > size.Height) ? translate : 0,
                //(size.Width < size.Height) ? translate : 0,
                0, 0,
                rotationDegree
            );
            return geometry;
        }
    }

    extension(Morph morph)
    {
        public StreamGeometry ToGeometry(Size size, double progress, double rotationDegree = 0, bool stretch = false)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.DrawMorph(morph, progress);
            }

            if (((size.Width <= 0 || size.Height <= 0) && rotationDegree == 0) || morph.MorphMatch.Count == 0)
                return geometry;

            var scale = Math.Min(size.Width, size.Height);

            geometry.Transform = CreateTransformMatrix(
                stretch ? size.Width : scale,
                stretch ? size.Height : scale,
                0,
                0,
                rotationDegree
            );
            return geometry;
        }
    }

    private static MatrixTransform CreateTransformMatrix(
        double scaleX,
        double scaleY,
        double translateX,
        double translateY,
        double rotationDegree
    )
    {
        var matrix = new Matrix(
            scaleX,
            0,
            0,
            scaleY,
            translateX,
            translateY);
        if (rotationDegree != 0)
        {
            var radians = rotationDegree * Math.PI / 180.0;
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);

            var offsetX = translateX * (1 - cos) + translateY * sin;
            var offsetY = translateY * (1 - cos) - translateX * sin;

            var rotationMatrix = new Matrix(
                cos, sin,
                -sin, cos,
                offsetX, offsetY);

            matrix *= rotationMatrix;
        }

        return new MatrixTransform(matrix);
    }
}