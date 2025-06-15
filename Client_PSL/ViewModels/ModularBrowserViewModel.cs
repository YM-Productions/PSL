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
using Utils;

namespace Client_PSL.ViewModels;

public class BrowsableObject
{
    public string Identifier { get => GetIdentifier(); }

    public InspectableObject inspectableObject { get; private set; }

    public BrowsableObject(InspectableObject inspectableObject)
    {
        this.inspectableObject = inspectableObject;
    }

    private string GetIdentifier()
    {
        if (inspectableObject.Properties.FirstOrDefault(prop => prop.Name.ToLower().Contains("name")) is InspectableProperty nameProperty)
        {
            return nameProperty.Value;
        }

        if (inspectableObject.Properties.FirstOrDefault(prop => prop.Name.ToLower().Contains("identity")) is InspectableProperty identityProperty)
        {
            return identityProperty.Value;
        }

        return inspectableObject.Properties.FirstOrDefault()?.Value ?? $"Unknown {inspectableObject.GetType().Name}";
    }
}

public class FilterProperty
{
    public string Name { get; }
    public bool IsSelected { get; set; }

    public FilterProperty(string name, bool isSelected = false)
    {
        Name = name;
        IsSelected = isSelected;
    }
}

public partial class ModularBrowserViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<BrowsableObject> _browsableObjects = new();
    // [ObservableProperty]
    // private IEnumerable<InspectableObject> _inspectableObjects;

    // [ObservableProperty]
    // private string _typeName;

    [ObservableProperty]
    private int _page;
    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private ViewModelBase _selectedView;
    // [ObservableProperty]
    // private string _selectedName = string.Empty;

    // public List<FilterProperty> FilterProperties { get; set; }

    public ModularBrowserViewModel()
    {
        // TypeName = nameof(PhysicalObject);
        // FilterProperties = InitializeFilterProperties();
    }

    private IEnumerable<InspectableObject> InitializeInspectableObjects(string name, string? parentFilter)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            if (parentFilter is not null && parentFilter.Length > 0)
            {
                foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Where(o => o.Name.Contains(name) && o.ParentIdentity.Contains(parentFilter)).Skip(Page * PageSize).Take(PageSize))
                {
                    yield return new(obj);
                }
            }
            else
            {
                foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Skip(Page * PageSize).Take(PageSize))
                {
                    if (obj.Name.Contains(name))
                    {
                        yield return new(obj);
                    }
                }
            }
        }
    }

    public void BrowseByName(string name, string? parentFilter)
    {
        BrowsableObjects.Clear();

        foreach (InspectableObject obj in InitializeInspectableObjects(name, parentFilter))
        {
            BrowsableObjects.Add(new(obj));
        }
    }

    public void SelectObject(InspectableObject inspectableObject)
    {
        SelectedView = new InspectableObjectViewModel(inspectableObject);
    }
}
