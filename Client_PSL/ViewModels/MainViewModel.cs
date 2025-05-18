using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private DebugViewModel _debugPage = new();
    [ObservableProperty]
    private ViewModelBase _mainPage = new LoginViewModel();

    public MainViewModel()
    {
        // Setup Singletons
        SpacetimeController spacetimeController = new();
    }

    public void ToggleDebug()
    {
        DebugPage.IsActive = !DebugPage.IsActive;
    }
}
