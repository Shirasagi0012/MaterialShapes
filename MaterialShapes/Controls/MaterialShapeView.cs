using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MaterialShapes;

namespace MaterialShapes.Controls;

public class MaterialShapeView : Decorator
{
    public static readonly StyledProperty<RoundedPolygon?> ShapeProperty =
        AvaloniaProperty.Register<MaterialShapeView, RoundedPolygon?>(nameof(Shape));

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<MaterialShapeView, IBrush?>(nameof(Background));

    private StreamGeometry? _geometryCache;
    private Size _lastSize;

    static MaterialShapeView()
    {
        AffectsRender<MaterialShapeView>(BackgroundProperty, ShapeProperty);
        ShapeProperty.Changed.AddClassHandler<MaterialShapeView>((x, _) => x.UpdateGeometry());
    }

    public RoundedPolygon? Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (!finalSize.Equals(_lastSize))
        {
            _lastSize = finalSize;
            UpdateGeometry();
        }

        return base.ArrangeOverride(finalSize);
    }

    public override void Render(DrawingContext context)
    {
        _geometryCache ??= Shape?.ToGeometry(_lastSize);

        if (Background is { } background && _geometryCache is { } geometry)
            context.DrawGeometry(background, null, geometry);

        base.Render(context);
    }

    private void UpdateGeometry()
    {
        _geometryCache = Shape?.ToGeometry(_lastSize);
        Clip = _geometryCache;
        InvalidateVisual();
    }
}
