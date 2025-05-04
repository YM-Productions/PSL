using CommunityToolkit.Mvvm.ComponentModel;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    DebugViewModel _debugPage = new();
}
