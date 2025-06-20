using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
using SpacetimeDB.Types;
using Client_PSL.Services;
using Utils;

namespace Client_PSL.ViewModels;

public partial class LandingPageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;
    [ObservableProperty]
    private ViewModelBase? _extendedViewModel;

    public LandingPageViewModel()
    {
        ISettings.Load();
        CurrentViewModel = new ModularBrowserViewModel(typeof(PhysicalObject));
    }

    // HACK: Temporarily set the current view model to a new instance of ModularBrowserViewModel
    public void SetExtendedViewModel(ViewModelBase vm)
    {
        ExtendedViewModel = vm;
    }
}
