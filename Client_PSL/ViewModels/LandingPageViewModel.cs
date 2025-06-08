using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
using Utils;

namespace Client_PSL.ViewModels;

public partial class LandingPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public LandingPageViewModel()
    {
        CurrentViewModel = new ModularBrowserViewModel();
    }
}
