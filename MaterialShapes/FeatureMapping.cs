using Avalonia;
using MeasuredFeatures = System.Collections.Generic.List<MaterialShapes.ProgressableFeature>;

namespace MaterialShapes;

internal record class ProgressableFeature (double Progress, Feature Feature);
internal record class DistanceVertex (double Distance, ProgressableFeature F1, ProgressableFeature F2);

public static class FeatureMapping
{
    internal static DoubleMapper FeatureMapper (MeasuredFeatures features1, MeasuredFeatures features2)
    {
        var filteredFeatures1 = features1
            .Where(f => f.Feature is CornerFeature)
            .ToList();

        var filteredFeatures2 = features2
            .Where(f => f.Feature is CornerFeature)
            .ToList();

        var featureProgressMapping = DoMapping(filteredFeatures1, filteredFeatures2);
        var dm = new DoubleMapper(featureProgressMapping.ToArray());
        return dm;
    }

    private static List<ValueTuple<double, double>> DoMapping (MeasuredFeatures features1, MeasuredFeatures features2)
    {
        var distanceVertexList = (from f1 in features1 from f2 in features2 let d = FeatureDistSquared(f1.Feature, f2.Feature) where d != float.MaxValue select new DistanceVertex(d, f1, f2)).ToList();

        distanceVertexList.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        if ( distanceVertexList.Count == 0 )
            return IdentityMapping;

        if ( distanceVertexList.Count == 1 )
        {
            var it = distanceVertexList[0];
            var f1 = it.F1.Progress;
            var f2 = it.F2.Progress;
            return
            [
                (f1, f2),
                ((f1 + 0.5D) % 1D, (f2 + 0.5D) % 1D)
            ];
        }

        var helper = new MappingHelper();
        foreach ( var vertex in distanceVertexList )
        {
            helper.AddMapping(vertex.F1, vertex.F2);
        }

        return helper.Mapping;
    }

    private static readonly List<ValueTuple<double, double>> IdentityMapping = [(0.0, 0.0), (0.5, 0.5)];

    class MappingHelper
    {
        public List<ValueTuple<double, double>> Mapping { get; } = [ ];
        private List<ProgressableFeature> _usedF1 = [];
        private List<ProgressableFeature> _usedF2 = [];

        public void AddMapping (ProgressableFeature f1, ProgressableFeature f2)
        {
            if ( _usedF1.Contains(f1) || _usedF2.Contains(f2) )
                return;

            var index = Mapping.Select(m => m.Item1)
                .ToList().BinarySearch(f1.Progress);

            if ( index >= 0 )
                throw new InvalidOperationException("There can't be two features with the same progress");

            int insertionIndex = ~index;
            int n = Mapping.Count;

            if ( n >= 1 )
            {
                var (before1, before2) = Mapping[(insertionIndex + n - 1) % n];
                var (after1, after2) = Mapping[insertionIndex % n];

                if ( DoubleMapper.ProgressDistance(f1.Progress, before1) < Utils.DistanceEpsilon ||
                    DoubleMapper.ProgressDistance(f1.Progress, after1) < Utils.DistanceEpsilon ||
                    DoubleMapper.ProgressDistance(f2.Progress, before2) < Utils.DistanceEpsilon ||
                    DoubleMapper.ProgressDistance(f2.Progress, after2) < Utils.DistanceEpsilon )
                {
                    return;
                }

                if ( n > 1 && !DoubleMapper.IsProgressInRange(f2.Progress, before2, after2) )
                {
                    return;
                }
            }

            Mapping.Insert(insertionIndex, (f1.Progress, f2.Progress));
            _usedF1.Add(f1);
            _usedF2.Add(f2);
        }
    }

    internal static double FeatureDistSquared (Feature f1, Feature f2)
    {
        // 1. 处理凹凸性匹配限制 (Concave-Convex matching)
        // 使用 C# 的模式匹配 (Pattern Matching) 语法同时检查类型和属性
        if ( f1 is CornerFeature c1 && f2 is CornerFeature c2 && c1.Convex != c2.Convex )
        {
            return Single.MaxValue;
        }

        // 2. 计算代表点之间的距离平方
        var p1 = FeatureRepresentativePoint(f1);
        var p2 = FeatureRepresentativePoint(f2);

        return (p1 - p2).GetDistanceSquared();
    }

    internal static Point FeatureRepresentativePoint (Feature feature)
    {
        // 获取第一个和最后一个 Cubic 曲线
        var firstCubic = feature.Cubics.First();
        var lastCubic = feature.Cubics.Last();

        // 计算起点和终点的中点作为代表点
        var x = (firstCubic.Anchor0.X + lastCubic.Anchor1.X) / 2D;
        var y = (firstCubic.Anchor0.Y + lastCubic.Anchor1.Y) / 2D;

        return new Point(x, y);
    }
}

internal class DoubleMapper
{
    internal static bool IsProgressInRange (double progress, double progressFrom, double progressTo) =>
        progressTo >= progressFrom
            ? progress >= progressFrom && progress <= progressTo
            : progress >= progressFrom || progress <= progressTo;

    private static double LinearMap (IReadOnlyList<double> xValues, IReadOnlyList<double> yValues, double x)
    {
        // TODO: Cleanup
        // x = Utils.PositiveModulo(x, 1d);
        // // Treat 1 as 0 (progress wraps).
        // if (x >= 1d) x = 0d;

        if ( x is < 0 or > 1 )
            throw new ArgumentOutOfRangeException(nameof(x));

        var segmentStartIndex = Enumerable.Range(0, xValues.Count)
            .First(i => IsProgressInRange(x, xValues[i], xValues[(i + 1) % xValues.Count]));
        var segmentEndIndex = (segmentStartIndex + 1) % xValues.Count;
        var segmentSizeX = Utils.PositiveModulo(xValues[segmentEndIndex] - xValues[segmentStartIndex], 1d);
        var segmentSizeY = Utils.PositiveModulo(yValues[segmentEndIndex] - yValues[segmentStartIndex], 1d);
        var positionInSegment = segmentSizeX < 0.001d
            ? 0.5d
            : Utils.PositiveModulo(x - xValues[segmentStartIndex], 1d) / segmentSizeX;
        return Utils.PositiveModulo(yValues[segmentStartIndex] + segmentSizeY * positionInSegment, 1d);
    }

    private static void ValidateProgress (List<double> p)
    {
        var prev = p.Last();
        var wraps = 0;
        foreach ( var curr in p )
        {
            if ( curr is < 0D or >= 1D )
                throw new ArgumentException(
                    $"FloatMapping - Progress outside of range: {string.Join(", ", p)}");

            if ( ProgressDistance(curr, prev) <= Utils.DistanceEpsilon )
                throw new ArgumentException(
                    $"FloatMapping - Progress repeats a value: {string.Join(", ", p)}");

            if ( curr < prev )
            {
                wraps++;
                if ( wraps > 1 )
                {
                    throw new ArgumentException(
                        $"FloatMapping - Progress wraps more than once: {string.Join(", ", p)}");
                }
            }

            prev = curr;
        }
    }

    internal static double ProgressDistance (double a, double b)
    {
        var diff = Math.Abs(a - b);
        return Math.Min(diff, 1 - diff);
    }

    private ValueTuple<double, double>[] _mappings;
    private List<double> _sourceValues;
    private List<double> _targetValues;

    public DoubleMapper (params ValueTuple<double, double>[ ] mappings)
    {
        _mappings = mappings;
        _sourceValues = new(_mappings.Length);
        _targetValues = new(_mappings.Length);
        foreach ( var m in mappings )
        {
            _sourceValues.Add(m.Item1);
            _targetValues.Add(m.Item2);
        }
        // Both source values and target values should be monotonically increasing, with the
        // exception of maybe one time (since progress wraps around).
        ValidateProgress(_sourceValues);
        ValidateProgress(_targetValues);
    }

    public double Map (double x) => LinearMap(_sourceValues, _targetValues, x);
    public double MapBack (double x) => LinearMap(_targetValues, _sourceValues, x);

    public static DoubleMapper Identity { get; } = new((0, 0), (0.5, 0.5));
}
