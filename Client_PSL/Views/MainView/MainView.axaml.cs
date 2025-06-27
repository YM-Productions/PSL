using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using SpacetimeDB.Types;
using Client_PSL.ViewModels;
using Client_PSL.Services;
using Client_PSL.Controls;
using Utils;

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

        Globals.smartViewHost = SmartHost;
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
        else if (e.Key == Key.F5)
        {
            SmartView sm = new SmartView()
            {
                Title = "ModularBrowser",
                InnerContent = new ModularBrowserViewModel(typeof(PhysicalObject)),

                Width = 600,
                Height = 600,
            };

            Globals.smartViewHost.AddSmartView(sm, new(100, 100));
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
