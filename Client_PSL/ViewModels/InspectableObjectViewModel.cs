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

    /// <summary>
    /// Initializes a new instance of the <see cref="InspectableObjectViewModel"/> class.
    /// Wraps the provided object in an <see cref="InspectableObject"/> if it is not already of that type.
    /// </summary>
    /// <param name="obj">
    /// The object to be inspected. If <paramref name="obj"/> is already an <see cref="InspectableObject"/>,
    /// it will be used directly; otherwise, a new <see cref="InspectableObject"/> will be created to wrap it.
    /// </param>
    public InspectableObjectViewModel(object obj)
    {
        if (obj.GetType() == typeof(InspectableObject)) _obj = (InspectableObject)obj;
        else _obj = new(obj);
    }
}
