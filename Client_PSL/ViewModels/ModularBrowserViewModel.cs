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

public class BrowserFilter
{
    public string Name { get; set; }
    public string Value { get; set; }

    public BrowserFilter(string name, string value = "")
    {
        Name = name;
        Value = value;
    }
}

public partial class ModularBrowserViewModel : ViewModelBase
{
    private static readonly Dictionary<Type, List<string>> TypeFilters = new() {
        { typeof(PhysicalObject), new() { "ParentIdentity", "Identity" } },
    };
    private readonly Dictionary<Type, Func<string, IEnumerable<InspectableObject>>> InspectableObjectInitializers;

    private Type selectedType;

    [ObservableProperty]
    private ObservableCollection<BrowsableObject> _browsableObjects = new();
    [ObservableProperty]
    private ObservableCollection<BrowserFilter> _filters = new();

    [ObservableProperty]
    private int _page;
    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private ViewModelBase? _selectedView;

    public ModularBrowserViewModel(Type type)
    {
        InspectableObjectInitializers = new Dictionary<Type, Func<string, IEnumerable<InspectableObject>>>
        {
            { typeof(PhysicalObject), InitializePhysicalObject }
        };

        selectedType = type;

        Filters.Clear();
        if (TypeFilters[type] is List<string> typeFilters)
        {
            foreach (string filterName in typeFilters)
            {
                Filters.Add(new(filterName));
            }
        }
    }

    public void BrowseByName(string name)
    {
        BrowsableObjects.Clear();

        if (InspectableObjectInitializers[selectedType] is Func<string, IEnumerable<InspectableObject>> initializer)
        {
            foreach (InspectableObject obj in initializer(name))
            {
                BrowsableObjects.Add(new(obj));
            }
        }
        else
        {
            Debug.LogError($"No initializer defined for type {selectedType.Name}");
        }
    }

    public void SelectObject(InspectableObject inspectableObject)
    {
        SelectedView = new InspectableObjectViewModel(inspectableObject);
    }

    #region Initializers

    private IEnumerable<InspectableObject> InitializePhysicalObject(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Where(o => o.Name.Contains(Identifier)
                    && o.ParentIdentity.Contains(Filters.FirstOrDefault(f => f.Name == "ParentIdentity")?.Value ?? string.Empty)
                    && o.Identity.Contains(Filters.FirstOrDefault(f => f.Name == "Identity")?.Value ?? string.Empty)
                    ).Skip(Page * PageSize).Take(PageSize))
            {
                yield return new(obj);
            }
        }
    }

    #endregion
}
