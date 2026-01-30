using Avalonia.Controls;
using Avalonia.Media;

namespace MaterialShapes.Gallery.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var testMDShape = RoundedPolygon.CreateStar(4, 150, 80, new(50), new(80), center: new(200, 200));
        var streamGeometry = new StreamGeometry();
        using (var ctx = streamGeometry.Open())
        {
            ctx.DrawRoundedPolygon(testMDShape);
        }

        TestBorder.Clip = streamGeometry;
    }
}