using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // Singleton access
    public static MainViewModel Instance { get; private set; }
    [ObservableProperty]
    private DebugViewModel _debugPage = new();
    [ObservableProperty]
    private ViewModelBase _mainPage;

    public MainViewModel()
    {
        if (Instance is not null)
        {
            throw new InvalidOperationException("MainViewModel instance already exists.");
        }
        Instance = this;

        // Setup Singletons
        SpacetimeController spacetimeController = new();
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
        MainPage = new LandingPageViewModel();
    }
}
