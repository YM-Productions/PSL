using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public DebugViewModel DebugPage => Globals.debugViewModel;
    public SettingsViewModel SettingsPage => new();

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
