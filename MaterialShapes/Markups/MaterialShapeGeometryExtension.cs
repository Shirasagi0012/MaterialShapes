using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace MaterialShapes.Markups;

public class MaterialShapeGeometryExtension : AvaloniaObject
{
    public MaterialShapeGeometryExtension(IBinding shape) : this()
    {
        this[!ShapeProperty] = shape;
    }

    public MaterialShapeGeometryExtension(RoundedPolygon shape) : this()
    {
        Shape = shape;
    }

    MaterialShapeGeometryExtension()
    {
        ShapeProperty.Changed.AddClassHandler<MaterialShapeGeometryExtension>((x, args) =>
        {
            if(args.NewValue is RoundedPolygon p)
                x.Geometry = new MaterialShapeGeometry(p);
        });
    }
    
    public static readonly StyledProperty<RoundedPolygon> ShapeProperty =
        AvaloniaProperty.Register<MaterialShapeGeometryExtension, RoundedPolygon>(nameof(Shape));

    private static readonly StyledProperty<MaterialShapeGeometry> GeometryProperty =
        AvaloniaProperty.Register<MaterialShapeGeometryExtension, MaterialShapeGeometry>(nameof(Geometry));
    
    public RoundedPolygon Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    private MaterialShapeGeometry Geometry
    {
        get => GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }
    
    public IBinding ProvideValue()
    {
        return this[!GeometryProperty];
    }
}
