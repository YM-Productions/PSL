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
    private int _page;
    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private ViewModelBase _selectedView;
    // [ObservableProperty]
    // private string _selectedName = string.Empty;

    public ModularBrowserViewModel()
    {
        TypeName = nameof(PhysicalObject);
    }

    public void BrowseByName(string name, string? parentFilter)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            PhysicalObjects.Clear();

            if (parentFilter is not null && parentFilter.Length > 0)
            {
                foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Where(o => o.Name.Contains(name) && o.ParentIdentity.Contains(parentFilter)).Skip(Page * PageSize).Take(PageSize))
                {
                    BrowsableObj browsableObj = new(obj.Name, obj.Identity, obj);
                    PhysicalObjects.Add(browsableObj);
                }
            }
            else
            {
                foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Skip(Page * PageSize).Take(PageSize))
                {
                    if (obj.Name.Contains(name))
                    {
                        BrowsableObj browsableObj = new(obj.Name, obj.Identity, obj);
                        PhysicalObjects.Add(browsableObj);
                    }
                }
            }
        }
    }

    public void SelectObject(string identity)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection &&
            connection.Db.PhysicalObject.Identity.Find(identity) is PhysicalObject obj)
        {
            // SelectedView = new PhysicalObjectViewModel(obj);
            SelectedView = new InspectableObjectViewModel(obj);
        }
    }
}
