using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MaterialShapes.Gallery.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] public partial RoundedPolygon? Shape { get; set; }


    public MainWindowViewModel()
    {
        Shape = RoundedPolygon.CreateStar(4, 150, 80, new(50), new(80), center: new(200, 200));
    }

    [RelayCommand]
    public void ChangeShape()
    {
        Shape = RoundedPolygon.CreateStar(5, 150, 80, new(50), new(80), center: new(200, 200));
    }
}
