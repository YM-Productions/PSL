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

public class BrowsableObj
{
    public string Name { get; set; }
    public string Identity { get; set; }

    public object OriginObj { get; private set; }
    public string OriginObjType { get => OriginObj.GetType().Name; }

    public BrowsableObj(string name, string identity, object originObj)
    {
        Name = name;
        Identity = identity;

        OriginObj = originObj;
    }
}

public partial class ModularBrowserViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<BrowsableObj> _physicalObjects = new();
    [ObservableProperty]
    private string _typeName;

    [ObservableProperty]
    private string _selectedName;

    public ModularBrowserViewModel()
    {
        // _physicalObjects = new() {
        //     new("Testy", "Test0"),
        //     new("Test1", "Test2"),
        //     new("Test3", "Test4"),
        // };

        TypeName = nameof(PhysicalObject);
    }

    // HACK: Only for testing purposes, should be removed in production.
    public void CreateBrowser()
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            PhysicalObjects.Clear();

            foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Skip(0).Take(50))
            {
                Debug.Log($"Adding {obj.Name}");
                BrowsableObj browsableObj = new(obj.Name, obj.Identity, obj);
                PhysicalObjects.Add(browsableObj);
            }
        }
    }

    public void BrowseByName(string name)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            PhysicalObjects.Clear();

            foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Skip(0).Take(50))
            {
                if (obj.Name.Contains(name))
                {
                    BrowsableObj browsableObj = new(obj.Name, obj.Identity, obj);
                    PhysicalObjects.Add(browsableObj);
                }
            }
        }
    }

    public void SetSelectedName(string name)
    {
        SelectedName = name;
    }
}
