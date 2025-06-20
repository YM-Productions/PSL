using Avalonia.Controls;
using Avalonia.Input;
using Client_PSL.ViewModels;
using Client_PSL.Services;

namespace Client_PSL.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F3 &&
            DataContext is MainViewModel vm)
        {
            vm.ToggleDebug();
        }
        else if (e.Key == Key.F2)
            ISettings.Data.Design.HighlightColor = ISettings.Data.Design.HighlightColor == Avalonia.Media.Color.Parse("#ff8066") ? Avalonia.Media.Color.Parse("#66ff80") : Avalonia.Media.Color.Parse("#ff8066");
    }
}
