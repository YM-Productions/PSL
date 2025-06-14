using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using SpacetimeDB.Types;
using Networking.SpacetimeController;
using Utils;

namespace Client_PSL.ViewModels;

public partial class InspectableObjectViewModel : ViewModelBase
{
    [ObservableProperty]
    private InspectableObject _obj;

    public InspectableObjectViewModel(object obj)
    {
        _obj = new(obj);
    }
}
