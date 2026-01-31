using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Media;

namespace MaterialShapes;

public class MaterialShapeGeometry : StreamGeometry
{
    public static readonly StyledProperty<RoundedPolygon?> ShapeProperty =
        AvaloniaProperty.Register<MaterialShapeGeometry, RoundedPolygon?>(nameof(Shape));

    static MaterialShapeGeometry()
    {
        AffectsGeometry(ShapeProperty);
        ShapeProperty.Changed.AddClassHandler<MaterialShapeGeometry>((x, _) => x.UpdateGeometry());
    }

    public RoundedPolygon? Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public MaterialShapeGeometry(RoundedPolygon? polygon)
    {
        Shape = polygon;
    }

    private void UpdateGeometry()
    {
        var shape = Shape;
        using var context = Open();
        if (shape != null) context.DrawRoundedPolygon(shape);
    }

    public override Geometry Clone()
    {
        var clone = new MaterialShapeGeometry(Shape)
        {
            Transform = Transform
        };
        return clone;
    }
}
