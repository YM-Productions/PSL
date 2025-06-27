using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using SpacetimeDB.Types;
using Client_PSL.ViewModels;
using Client_PSL.Services;
using Client_PSL.Controls;

namespace Client_PSL.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        this.GetObservable(BoundsProperty).Subscribe(bounds =>
        {
            SmartHost.Width = bounds.Width;
            SmartHost.Height = bounds.Height;
        });

        SmartView loginView = new()
        {
            Title = "Login",
            InnerContent = new LoginViewModel(),
            MinWidth = 300,
            MinHeight = 300,
            MaxWidth = 600,
            MaxHeight = 600,
        };

        SmartView registerView = new()
        {
            Title = "Register",
            InnerContent = new RegisterViewModel(),
            MinWidth = 300,
            MinHeight = 300,
        };

        SmartHost.AddSmartView(loginView, new Avalonia.Point(100, 100));
        SmartHost.AddSmartView(registerView, new Avalonia.Point(400, 100));
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
