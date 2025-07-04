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

public class SerializableSmartView
{
    public string Title { get; set; } = "SmartView";
    public ViewModelBase InnerViewModel { get; set; } = new ErrorViewModel("Unknown ViewModel");
    public double Left { get; set; } = 0;
    public double Top { get; set; } = 0;
    public double Height { get; set; } = 100;
    public double Width { get; set; } = 100;
}

public class SmartViewHost : Canvas
{
    public static readonly string SaveName = "SmarViewConfig";

    public SmartViewHost()
    {
        LoadConfig();
    }

    public void AddSmartView(SmartView view, Point position)
    {
        Children.Add(view);
        SetLeft(view, position.X);
        SetTop(view, position.Y);
    }

    public void BringToFront(SmartView view)
    {
        Children.Remove(view);
        Children.Add(view);
    }

    public void Clear()
    {
        List<SmartView> removeList = Children.OfType<SmartView>().ToList();
        foreach (SmartView view in removeList)
            Children.Remove(view);
    }

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
