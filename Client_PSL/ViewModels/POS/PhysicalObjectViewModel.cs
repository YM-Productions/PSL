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

public partial class PhysicalObjectViewModel : ViewModelBase
{
    public PhysicalObject physicalObject;

    public string Identity => physicalObject.Identity;
    public string IsStatic => physicalObject.IsStatic ? "Foundation" : "Vehicle";
    public string Name => physicalObject.Name;
    public string ParentIdentity => physicalObject.ParentIdentity;
    public string Position => $"{physicalObject.XPos} | {physicalObject.YPos}";

    public PhysicalObjectViewModel(PhysicalObject physicalObject)
    {
        this.physicalObject = physicalObject;
    }
}
