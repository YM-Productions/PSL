using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Networking.SpacetimeController;
using SpacetimeDB.Types;
using Utils;

namespace Client_PSL.ViewModels;

public partial class LandingPageViewModel : ViewModelBase
{
    public static LandingPageViewModel Instance { get; private set; }

    [ObservableProperty]
    private ViewModelBase _currentViewModel;
    [ObservableProperty]
    private ViewModelBase? _extendedViewModel;

    public LandingPageViewModel()
    {
        if (Instance is not null)
            throw new InvalidOperationException("MainViewModel instance already exists.");
        Instance = this;

        CurrentViewModel = new ModularBrowserViewModel(typeof(PhysicalObject));
    }

    // HACK: Temporarily set the current view model to a new instance of ModularBrowserViewModel
    public void SetExtendedViewModel(ViewModelBase vm)
    {
        ExtendedViewModel = vm;
    }
}
