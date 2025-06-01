using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using SpacetimeDB.Types;
using Networking.SpacetimeController;
using Utils;

namespace Client_PSL.ViewModels;

public partial class POSBrowserViewModel : ViewModelBase
{
    [ObservableProperty]
    public ObservableCollection<PhysicalObject> physicalObjects;

    public POSBrowserViewModel()
    {
        physicalObjects = new ObservableCollection<PhysicalObject>();
    }

    public void BrowsePhysicalObjects()
    {
        if (SpacetimeController.Instance.GetConnection() is not DbConnection connection)
            throw new NotImplementedException("Handle Connection not established");

        foreach (PhysicalObject po in connection.Db.PhysicalObject.IdxPhysicalobjectParentid.Filter("0"))
            Debug.Log(po.Name);
    }

    public void CreatePhysicalObject()
    {
        if (SpacetimeController.Instance.GetConnection() is not DbConnection connection)
            throw new NotImplementedException("Handle Connection not established");

        connection.Reducers.CreateFoundation(Guid.NewGuid().ToString()[0..8], "0", 0, 0);
    }
}
