using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using SpacetimeDB.Types;
using Networking.SpacetimeController;
using Client_PSL.Services;
using Client_PSL.Controls;
using Utils;

namespace Client_PSL.ViewModels;

public partial class InspectableObjectViewModel : ViewModelBase
{
    [ObservableProperty]
    private InspectableObject _obj;

    private static readonly Dictionary<Type, Func<object, ViewModelBase>> _viewModelFactories = new() {
        { typeof(PhysicalObject), obj => new PhysicalObjectViewModel((PhysicalObject)obj)},
    };

    /// <summary>
    /// Gets a value indicating whether a dedicated view model exists for the type of the underlying object.
    /// Returns <c>true</c> if a view model factory is registered for the object's type, allowing for specialized
    /// presentation or interaction logic; otherwise, returns <c>false</c>.
    /// </summary>
    public bool HasViewModel => _viewModelFactories.ContainsKey(_obj.Obj.GetType());

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

    /// <summary>
    /// Opens the extended view for the underlying object by creating and displaying its associated view model,
    /// if a dedicated view model factory is registered for the object's type. If no factory is found, the method does nothing.
    /// This allows for specialized inspection or editing of the object in the application's landing page.
    /// </summary>
    public void OpenViewModel()
    {
        if (_viewModelFactories[_obj.Obj.GetType()] is Func<object, ViewModelBase> factory)
        {
            ViewModelBase obj = factory(_obj.Obj);
            SmartView sv = new()
            {
                Title = obj.GetType().Name,
                InnerContent = obj,
            };

            Globals.smartViewHost.AddSmartView(sv, new(0, 0));
        }
    }
}
