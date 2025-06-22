using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
        {
            SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
        }
    }

    private void OnPopupKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && sender is Popup popup)
        {
            popup.IsOpen = false;
            if (popup.IsOpen)
                popup.Focus();
        }
    }
}
