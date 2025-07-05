using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Client_PSL.Services;
using Client_PSL.ViewModels;
using Utils;

namespace Client_PSL.Controls;

/// <summary>
/// Represents a serializable configuration for a <see cref="SmartView"/>.
/// Stores window title, view model, and geometry for persistence.
/// </summary>
public class SerializableSmartView
{
    /// <summary>
    /// Gets or sets the title of the SmartView window.
    /// </summary>
    public string Title { get; set; } = "SmartView";

    /// <summary>
    /// Gets or sets the view model to be displayed in the SmartView.
    /// If deserialization fails, an <see cref="ErrorViewModel"/> is used.
    /// </summary>
    public ViewModelBase InnerViewModel { get; set; } = new ErrorViewModel("Unknown ViewModel");

    /// <summary>
    /// Gets or sets the left position of the SmartView within its host.
    /// </summary>
    public double Left { get; set; } = 0;

    /// <summary>
    /// Gets or sets the top position of the SmartView within its host.
    /// </summary>
    public double Top { get; set; } = 0;

    /// <summary>
    /// Gets or sets the height of the SmartView.
    /// </summary>
    public double Height { get; set; } = 100;

    /// <summary>
    /// Gets or sets the width of the SmartView.
    /// </summary>
    public double Width { get; set; } = 100;
}

/// <summary>
/// Hosts multiple <see cref="SmartView"/> windows on a canvas.
/// Provides methods for adding, removing, arranging, and persisting SmartViews.
/// </summary>
public class SmartViewHost : Canvas
{
    private static Logger logger = Logger.LoggerFactory.CreateLogger(nameof(SmartViewHost));

    /// <summary>
    /// The file name used to save and load SmartView configurations.
    /// </summary>
    public static readonly string SaveName = "SmarViewConfig";

    private static List<Type> SaveIgnoredViews = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartViewHost"/> class and loads configuration.
    /// </summary>
    public SmartViewHost()
    {
        logger.SetLevel(50);
        LoadConfig("default");

        logger.Log("Initialized SmartViewHost with default configuration.");
    }

    /// <summary>
    /// Registers a view type to be ignored by the SmartView host when saving views.
    /// If the specified view type is not already in the list of ignored views, it will be added.
    /// This is useful for excluding certain views from being persisted or processed by the SmartView system.
    /// </summary>
    public static void RegisterIgnoredView(Type viewType)
    {
        if (!SaveIgnoredViews.Contains(viewType))
            SaveIgnoredViews.Add(viewType);

        logger.Log($"Registered ignored view type: {viewType.Name}");
    }

    /// <summary>
    /// Adds a <see cref="SmartView"/> to the host at the specified position.
    /// </summary>
    /// <param name="view">The SmartView to add.</param>
    /// <param name="position">The position to place the SmartView.</param>
    public void AddSmartView(SmartView view, Point position)
    {
        Children.Add(view);
        SetLeft(view, position.X);
        SetTop(view, position.Y);

        logger.Log($"Added SmartView '{view.Title}' at position {position} with size {view.Width}x{view.Height}.");
    }

    /// <summary>
    /// Brings the specified <see cref="SmartView"/> to the front of the z-order.
    /// </summary>
    /// <param name="view">The SmartView to bring to front.</param>
    public void BringToFront(SmartView view)
    {
        Children.Remove(view);
        Children.Add(view);

        logger.Log($"Brought SmartView '{view.Title}' to the front.");
    }

    /// <summary>
    /// Removes all <see cref="SmartView"/> instances from the host.
    /// </summary>
    public void Clear()
    {
        List<SmartView> removeList = Children.OfType<SmartView>().ToList();
        foreach (SmartView view in removeList)
            Children.Remove(view);

        logger.Log($"Cleared {removeList.Count} SmartViews from the host.");
    }

    /// <summary>
    /// Ensures all SmartViews are within the visible bounds of the host.
    /// Crops or repositions windows that extend beyond the host area.
    /// </summary>
    public void CropAllToView()
    {
        foreach (SmartView view in Children.OfType<SmartView>())
        {
            if (GetLeft(view) + view.Width > Bounds.Width)
            {
                SetLeft(view, Bounds.Width - Bounds.Width);
                if (view.Width > Bounds.Width)
                    SetLeft(view, 0);
            }

            if (GetTop(view) + view.Height > Bounds.Height)
            {
                SetBottom(view, Bounds.Height - view.Height);
                if (view.Height > Bounds.Height)
                    SetTop(view, 0);
            }

            logger.Log($"Cropped SmartView '{view.Title}' to fit within host bounds. Position: ({GetLeft(view)}, {GetTop(view)}), Size: {view.Width}x{view.Height}.");
        }
    }

    /// <summary>
    /// Loads SmartView configuration from persistent storage and restores all SmartViews.
    /// </summary>
    public void LoadConfig(string configName)
    {
        logger.Log($"Loading SmartView configuration '{configName}' from {SaveName}.");
        Clear();

        if (Globals.fileService.ReadText(SaveName) is not string json)
        {
            Debug.LogWarning("Failed to read SmartView configuration file.");
            return;
        }

        if (JsonSerializer.Deserialize<Dictionary<string, List<SerializableSmartView>>>(json, ISettings.JsonOptions) is not Dictionary<string, List<SerializableSmartView>> views)
        {
            Debug.LogWarning("Failed to deserialize SmartView configuration.");
            return;
        }

        if (views.Count == 0)
            SaveConfig(configName);

        if (!views.ContainsKey(configName))
        {
            Debug.LogWarning($"Configuration '{configName}' does not exist. Using default configuration.");
            return;
        }

        foreach (SerializableSmartView serializable in views[configName])
        {
            SmartView view = new()
            {
                Title = serializable.Title,
                InnerContent = serializable.InnerViewModel,
                Height = serializable.Height,
                Width = serializable.Width,
            };
            AddSmartView(view, new Point(serializable.Left, serializable.Top));

            logger.Log($"Loaded SmartView '{serializable.Title}' with size {serializable.Width}x{serializable.Height} at position ({serializable.Left}, {serializable.Top}).");
        }

        CropAllToView();
        logger.Log($"Loaded {views[configName].Count} SmartViews from configuration '{configName}'.");
    }

    /// <summary>
    /// Saves the current SmartView configuration to persistent storage.
    /// </summary>
    public void SaveConfig(string configName)
    {
        logger.Log($"Saving SmartView configuration '{configName}' to {SaveName}.");
        Dictionary<string, List<SerializableSmartView>> views = new();

        if (Globals.fileService.ReadText(SaveName) is string json &&
            JsonSerializer.Deserialize<Dictionary<string, List<SerializableSmartView>>>(json, ISettings.JsonOptions) is Dictionary<string, List<SerializableSmartView>> existingViews)
            views = existingViews;

        views[configName] = new();

        foreach (SmartView view in Children)
        {
            ViewModelBase innerContent = view.InnerContent ?? new ErrorViewModel("Unknown ViewModel");

            if (SaveIgnoredViews.Contains(innerContent.GetType()))
                continue;

            SerializableSmartView serializable = new()
            {
                Title = view.Title,
                InnerViewModel = innerContent,
                Left = GetLeft(view),
                Top = GetTop(view),
                Height = view.Height,
                Width = view.Width,
            };
            views[configName].Add(serializable);
        }
        logger.Log($"Saving {views.Count} SmartViews to {SaveName}");

        Globals.fileService.WriteText(SaveName, JsonSerializer.Serialize(views, ISettings.JsonOptions));
        logger.Log($"Saved {views[configName].Count} SmartViews to configuration '{configName}' in {SaveName}.");
    }

    /// <summary>
    /// Deletes the specified SmartView configuration by name.
    /// This method ensures that the default configuration cannot be deleted and that the configuration exists before attempting removal.
    /// If the configuration is successfully removed, the updated configurations are saved to persistent storage.
    /// Logs an error if the configuration is the default or does not exist.
    /// </summary>
    /// <param name="configName">The name of the configuration to delete.</param>
    public void DeleteConfig(string configName)
    {
        logger.Log($"Attempting to delete SmartView configuration '{configName}'.");

        if (ISettings.Data.SmartView.DefaultConfigName == configName)
        {
            Debug.LogWarning("Cannot delete the default configuration.");
            return;
        }

        if (Globals.fileService.ReadText(SaveName) is not string json)
        {
            Debug.LogWarning("Failed to read SmartView configuration file for deletion.");
            return;
        }

        if (JsonSerializer.Deserialize<Dictionary<string, List<SerializableSmartView>>>(json, ISettings.JsonOptions) is not Dictionary<string, List<SerializableSmartView>> views)
        {
            Debug.LogWarning("Failed to deserialize SmartView configuration for deletion.");
            return;
        }

        if (!views.ContainsKey(configName))
        {
            Debug.LogWarning($"Configuration '{configName}' does not exist.");
            return;
        }

        views.Remove(configName);
        Globals.fileService.WriteText(SaveName, JsonSerializer.Serialize(views, ISettings.JsonOptions));
        logger.Log($"Deleted SmartView configuration '{configName}' successfully.");
    }
}
