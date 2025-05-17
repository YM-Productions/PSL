using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    DebugViewModel _debugPage = new();

    public MainViewModel()
    {
        // Setup Singletons test
        SpacetimeController spacetimeController = new();
    }

    public void ToggleDebug()
    {
        DebugPage.IsActive = !DebugPage.IsActive;
    }
}
