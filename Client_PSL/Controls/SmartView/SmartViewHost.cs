using Avalonia;
using Avalonia.Controls;
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
    /// <summary>
    /// The file name used to save and load SmartView configurations.
    /// </summary>
    public static readonly string SaveName = "SmarViewConfig.json";

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartViewHost"/> class and loads configuration.
    /// </summary>
    public SmartViewHost()
    {
        LoadConfig();
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
    }

    /// <summary>
    /// Brings the specified <see cref="SmartView"/> to the front of the z-order.
    /// </summary>
    /// <param name="view">The SmartView to bring to front.</param>
    public void BringToFront(SmartView view)
    {
        Children.Remove(view);
        Children.Add(view);
    }

    /// <summary>
    /// Removes all <see cref="SmartView"/> instances from the host.
    /// </summary>
    public void Clear()
    {
        List<SmartView> removeList = Children.OfType<SmartView>().ToList();
        foreach (SmartView view in removeList)
            Children.Remove(view);
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
        }
    }

    /// <summary>
    /// Loads SmartView configuration from persistent storage and restores all SmartViews.
    /// </summary>
    public void LoadConfig()
    {
        Clear();

        if (Globals.fileService.ReadText(SaveName) is not string json)
            return;

        if (JsonSerializer.Deserialize<List<SerializableSmartView>>(json, ISettings.JsonOptions) is not List<SerializableSmartView> views)
            return;

        foreach (SerializableSmartView serializable in views)
        {
            SmartView view = new()
            {
                Title = serializable.Title,
                InnerContent = serializable.InnerViewModel,
                Height = serializable.Height,
                Width = serializable.Width,
            };
            AddSmartView(view, new Point(serializable.Left, serializable.Top));
        }

        CropAllToView();
    }

    /// <summary>
    /// Saves the current SmartView configuration to persistent storage.
    /// </summary>
    public void SaveConfig()
    {
        List<SerializableSmartView> views = new();
        foreach (SmartView view in Children)
        {
            SerializableSmartView serializable = new()
            {
                Title = view.Title,
                InnerViewModel = view.InnerContent ?? new ErrorViewModel("Unknonw ViewModel"),
                Left = GetLeft(view),
                Top = GetTop(view),
                Height = view.Height,
                Width = view.Width,
            };
            views.Add(serializable);
        }
        Debug.Log($"Saving {views.Count} SmartViews to {SaveName}");

        Globals.fileService.WriteText(SaveName, JsonSerializer.Serialize(views, ISettings.JsonOptions));
    }
}
