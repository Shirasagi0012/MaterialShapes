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
}
