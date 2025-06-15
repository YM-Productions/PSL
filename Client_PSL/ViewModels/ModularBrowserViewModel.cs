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
        { typeof(PhysicalObject), new() { "Identity", "ParentIdenitiy" } },
        { typeof(Account), new() { "Identity", "MailAddress" } },
        { typeof(Admin), new() { "Name" } },
        { typeof(ClientDebugLog), new() { "Layer", "Message" } },
        { typeof(Hardpoint), new() { "ParentIdentity", "Size" } },
        { typeof(HardpointPermission), new() { "AccountIdentity", "HardpointIdentity", "Size" } },
        { typeof(PhysicalObjectPermission), new() { "AccountIdentity", "PhysicalObjectIdentity", "Size" } },
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
            { typeof(PhysicalObject), InitializePhysicalObject },
            { typeof(Account), InitializeAccount },
            { typeof(Admin), InitializeAdmin },
            { typeof(ClientDebugLog), InitializeClienDebugLog },
            { typeof(Hardpoint), InitializeHardpoint },
            { typeof(HardpointPermission), InitializeHardpointPermission },
            { typeof(PhysicalObjectPermission), InitializePhysicalObjectPermission },
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

    public void BrowseByIdentifier(string identifier)
    {
        BrowsableObjects.Clear();

        Debug.Log($"Browsing {selectedType.Name} with identifier: {identifier}, Page: {Page}, PageSize: {PageSize}");
        if (InspectableObjectInitializers[selectedType] is Func<string, IEnumerable<InspectableObject>> initializer)
        {
            Debug.Log($"Browsing {selectedType.Name} with identifier: {identifier}");
            foreach (InspectableObject obj in initializer(identifier))
            {
                Debug.Log($"Adding object: {obj.GetType().Name}");
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

    private IEnumerable<InspectableObject> InitializeAccount(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            foreach (Account obj in connection.Db.Account.Iter().Where(o => o.UserName.Contains(Identifier)
                    && o.Identity.ToString().Contains(Filters.FirstOrDefault(f => f.Name == "Identity")?.Value ?? string.Empty)
                    && o.MailAddress.Contains(Filters.FirstOrDefault(f => f.Name == "MailAddress")?.Value ?? string.Empty)
                    ).Skip(Page * PageSize).Take(PageSize))
            {
                yield return new(obj);
            }
        }
    }

    private IEnumerable<InspectableObject> InitializeAdmin(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            foreach (Admin obj in connection.Db.Admin.Iter().Where(o => o.Identity.ToString().Contains(Identifier)
                    && o.Name.Contains(Filters.FirstOrDefault(f => f.Name == "Name")?.Value ?? string.Empty)
                    ).Skip(Page * PageSize).Take(PageSize))
            {
                yield return new(obj);
            }
        }
    }

    private IEnumerable<InspectableObject> InitializeClienDebugLog(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            int.TryParse(Filters.FirstOrDefault(f => f.Name == "Layer")?.Value, out int layer);

            foreach (ClientDebugLog obj in connection.Db.ClientDebugLog.Iter().Where(o => o.Identity.ToString().Contains(Identifier)
                    && (layer == 0 || o.Layer == layer)
                    && o.Message.Contains(Filters.FirstOrDefault(f => f.Name == "Message")?.Value ?? string.Empty)
                    ).Skip(Page * PageSize).Take(PageSize))
            {
                yield return new(obj);
            }
        }
    }

    private IEnumerable<InspectableObject> InitializeHardpoint(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            int.TryParse(Filters.FirstOrDefault(f => f.Name == "Size")?.Value, out int size);

            foreach (Hardpoint obj in connection.Db.Hardpoint.Iter().Where(o => o.Identity.ToString().Contains(Identifier)
                    && o.PhysicalObjectIdentity.Contains(Filters.FirstOrDefault(f => f.Name == "ParentIdentity")?.Value ?? string.Empty)
                    && (size == 0 || o.Size == size)
                    ).Skip(Page * PageSize).Take(PageSize))
            {
                yield return new(obj);
            }
        }
    }

    private IEnumerable<InspectableObject> InitializeHardpointPermission(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            int.TryParse(Filters.FirstOrDefault(f => f.Name == "Level")?.Value, out int level);

            foreach (HardpointPermission obj in connection.Db.HardpointPermission.Iter().Where(o => o.Identity.Contains(Identifier)
                    && o.AccountIdentity.ToString().Contains(Filters.FirstOrDefault(f => f.Name == "AccountIdentity")?.Value ?? string.Empty)
                    && o.HardpointIdentity.ToString().Contains(Filters.FirstOrDefault(f => f.Name == "HardpointIdentity")?.Value ?? string.Empty)
                    && (level == 0 || o.Level == level)
                    ).Skip(Page * PageSize).Take(PageSize))
            {
                yield return new(obj);
            }
        }
    }

    private IEnumerable<InspectableObject> InitializePhysicalObjectPermission(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            int.TryParse(Filters.FirstOrDefault(f => f.Name == "Level")?.Value, out int level);

            foreach (PhysicalObjectPermission obj in connection.Db.PhysicalObjectPermission.Iter().Where(o => o.Identity.Contains(Identifier)
                    && o.AccountIdentity.ToString().Contains(Filters.FirstOrDefault(f => f.Name == "AccountIdentity")?.Value ?? string.Empty)
                    && o.PhysicalObjectIdentity.ToString().Contains(Filters.FirstOrDefault(f => f.Name == "PhysicalObjectIdentity")?.Value ?? string.Empty)
                    && (level == 0 || o.Level == level)
                    ).Skip(Page * PageSize).Take(PageSize))
            {
                yield return new(obj);
            }
        }
    }

    #endregion
}
