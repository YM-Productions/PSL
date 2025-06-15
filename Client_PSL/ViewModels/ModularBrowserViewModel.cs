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

/// <summary>
/// Represents a wrapper for an <see cref="InspectableObject"/> that provides a human-readable identifier
/// based on the object's properties. The identifier is determined by searching for a property containing
/// "name" or "identity" in its name, or falls back to the first property value or a default string.
/// </summary>
public class BrowsableObject
{
    /// <summary>
    /// Gets a human-readable identifier for the underlying <see cref="InspectableObject"/>.
    /// The identifier is determined by searching for a property whose name contains "name" or "identity"
    /// (case-insensitive). If neither is found, the value of the first property is used, or a default string
    /// indicating the type is returned.
    /// </summary>
    public string Identifier { get => GetIdentifier(); }

    /// <summary>
    /// Gets the underlying <see cref="InspectableObject"/> instance that is being wrapped.
    /// </summary>
    public InspectableObject inspectableObject { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsableObject"/> class with the specified
    /// <see cref="InspectableObject"/>.
    /// </summary>
    /// <param name="inspectableObject">
    /// The <see cref="InspectableObject"/> to be wrapped by this <see cref="BrowsableObject"/>.
    /// </param>
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

/// <summary>
/// Represents a filter used in the browser to narrow down displayed items based on a specific property and value.
/// </summary>
public class BrowserFilter
{
    /// <summary>
    /// Gets or sets the name of the property to filter by.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value to filter for the specified property.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserFilter"/> class with the specified property name and optional value.
    /// </summary>
    /// <param name="name">The name of the property to filter by.</param>
    /// <param name="value">The value to filter for the specified property. Defaults to an empty string.</param>
    public BrowserFilter(string name, string value = "")
    {
        Name = name;
        Value = value;
    }
}

/// <summary>
/// View model for the modular browser component, responsible for managing and displaying
/// collections of browsable objects, handling filtering, selection, and navigation logic.
/// Provides mechanisms to initialize, filter, and select objects of various types for inspection.
/// </summary>
public partial class ModularBrowserViewModel : ViewModelBase
{
    private Logger logger;

    private static readonly Dictionary<Type, List<string>> TypeFilters = new() {
        { typeof(PhysicalObject), new() { "Identity", "ParentIdentity", "IsStatic" } },
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ModularBrowserViewModel"/> class for the specified type.
    /// Sets up the available filters and object initializers based on the provided type.
    /// </summary>
    /// <param name="type">
    /// The <see cref="Type"/> of objects to be browsed and displayed in the view model.
    /// </param>
    public ModularBrowserViewModel(Type type)
    {
        logger = Logger.LoggerFactory.GetLogger("ModularBrowser");
        logger.SetLevel(50);

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

    /// <summary>
    /// Loads and displays objects of the selected type that match the specified identifier.
    /// Clears the current list and populates it with the results from the appropriate initializer.
    /// </summary>
    /// <param name="identifier">
    /// The identifier used to filter or locate objects of the selected type.
    /// </param>
    public void BrowseByIdentifier(string identifier)
    {
        logger.Log($"Browsing objects of type {selectedType.Name} with identifier: {identifier}");
        BrowsableObjects.Clear();
        logger.Log($"Cleared existing browsable objects. Current count: {BrowsableObjects.Count}");

        if (InspectableObjectInitializers[selectedType] is Func<string, IEnumerable<InspectableObject>> initializer)
        {
            logger.Log($"Using initializer for type {selectedType.Name}");
            foreach (InspectableObject obj in initializer(identifier))
            {
                logger.Log($"Adding object: {obj}");
                BrowsableObjects.Add(new(obj));
            }
        }
        else
        {
            logger.Log($"No initializer defined for type {selectedType.Name}");
            Debug.LogError($"No initializer defined for type {selectedType.Name}");
        }
    }

    /// <summary>
    /// Selects the specified <see cref="InspectableObject"/> and creates a corresponding view model for detailed inspection.
    /// </summary>
    /// <param name="inspectableObject">
    /// The <see cref="InspectableObject"/> to be selected and displayed in detail.
    /// </param>
    public void SelectObject(InspectableObject inspectableObject)
    {
        logger.Log($"Selecting object: {inspectableObject}");
        SelectedView = new InspectableObjectViewModel(inspectableObject);
    }

    #region Initializers

    private IEnumerable<InspectableObject> InitializePhysicalObject(string Identifier)
    {
        if (SpacetimeController.Instance.GetConnection() is DbConnection connection)
        {
            bool.TryParse(Filters.FirstOrDefault(f => f.Name == "IsStatic")?.Value, out bool isStatic);

            foreach (PhysicalObject obj in connection.Db.PhysicalObject.Iter().Where(o => o.Name.Contains(Identifier)
                    && o.ParentIdentity.Contains(Filters.FirstOrDefault(f => f.Name == "ParentIdentity")?.Value ?? string.Empty)
                    && o.Identity.Contains(Filters.FirstOrDefault(f => f.Name == "Identity")?.Value ?? string.Empty)
                    && (string.IsNullOrEmpty(Filters.FirstOrDefault(f => f.Name == "IsStatic")?.Value) ? true : o.IsStatic == isStatic)
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
