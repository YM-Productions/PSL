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

public class MyObj
{
    public string Name { get; set; }
    public string Identity { get; set; }

    public MyObj(string name, string identity)
    {
        Name = name;
        Identity = identity;
    }
}

public partial class ModularBrowserViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<MyObj> _physicalObjects;

    public ModularBrowserViewModel()
    {
        // _physicalObjects = new() {
        //     new("Testy", "Test0"),
        //     new("Test1", "Test2"),
        //     new("Test3", "Test4"),
        // };
    }

    public void CreateBrowser()
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            PhysicalObjects = new();

            foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Skip(0).Take(50))
            {
                Debug.Log($"Adding {obj.Name}");
                MyObj myObj = new(obj.Name, obj.Identity);
                PhysicalObjects.Add(myObj);
            }
        }
    }
}
