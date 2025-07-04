using Avalonia;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SpacetimeDB.Types;
using Networking.SpacetimeController;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.ViewModels;

public class ViewOpener
{
    public Type ViewModelType;
    public string Name { get => ViewModelType.Name.Replace("ViewModel", ""); }

    public ViewOpener(Type viewModelType)
    {
        ViewModelType = viewModelType;
    }
}

public partial class MainViewModel : ViewModelBase
{
    public DebugViewModel DebugPage => Globals.debugViewModel;
    public SettingsViewModel SettingsPage => new();

    public static ObservableCollection<ViewOpener> ViewOpeners { get; } = new()
    {
        new ViewOpener(typeof(ModularBrowserViewModel)),
    };

    [ObservableProperty]
    private ViewModelBase? _mainPage;

    [ObservableProperty]
    private bool _isLanded = false;
    [ObservableProperty]
    private bool _isNotLanded = true;

    public MainViewModel()
    {
        // _debugPage = Globals.debugViewModel;

        SetLoginPage();
    }

    public static void AddViewOpener(Type viewModelType)
    {
        if (!typeof(ViewModelBase).IsAssignableFrom(viewModelType))
            throw new ArgumentException($"Type {viewModelType.Name} is not a ViewModelBase type.");

        if (ViewOpeners.Where(v => v.ViewModelType == viewModelType).Any())
            return;

        ViewOpeners.Add(new ViewOpener(viewModelType.GetType()));
    }

    public void ToggleDebug()
    {
        DebugPage.IsActive = !DebugPage.IsActive;
    }

    public void SetLoginPage()
    {
        MainPage = new LoginViewModel();
    }
    public void SetRegisterPage()
    {
        MainPage = new RegisterViewModel();
    }

    public void SetLandingPage()
    {
        // MainPage = Globals.landingPageViewModel;
        IsLanded = true;
        IsNotLanded = false;
    }
}
