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

// HACK: Temporary observable Property Obj
public class ObservableHardpoint
{
    private Hardpoint _hardpoint;
    public string Identity { get => _hardpoint.Identity[0..8]; }
    public string PhysicalObjectIdentity { get => _hardpoint.PhysicalObjectIdentity[0..8]; }
    public int Size { get => _hardpoint.Size; }

    public ObservableHardpoint(Hardpoint hardpoint)
    {
        this._hardpoint = hardpoint;
    }
}

public partial class PhysicalObjectViewModel : ViewModelBase
{
    public PhysicalObject physicalObject;

    public string Identity => physicalObject.Identity;
    public string IsStatic => physicalObject.IsStatic ? "Foundation" : "Vehicle";
    public string Name => physicalObject.Name;
    public string ParentIdentity => physicalObject.ParentIdentity;
    public string Position => $"{physicalObject.XPos} | {physicalObject.YPos}";

    [ObservableProperty]
    private ObservableCollection<ObservableHardpoint> _hardpoints = new();
    public int Count => Hardpoints.Count;

    public PhysicalObjectViewModel(PhysicalObject physicalObject)
    {
        this.physicalObject = physicalObject;

        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            Hardpoints.Clear();
            foreach (Hardpoint hardpoint in connection.Db.Hardpoint.IdxHardpointPhysicalobjectidentity.Filter(physicalObject.Identity))
            {
                Hardpoints.Add(new(hardpoint));
            }
        }
    }
}
