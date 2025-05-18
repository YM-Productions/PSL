using Avalonia.Controls;
using Avalonia.Input;
using Client_PSL.ViewModels;

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
    }
}
