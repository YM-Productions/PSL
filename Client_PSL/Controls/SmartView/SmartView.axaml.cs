using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Utils;

namespace Client_PSL.Controls;

public partial class SmartView : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SmartView, string>(nameof(Title), "Window");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? InnerContent
    {
        get => ContentHost.DataContext;
        set => ContentHost.DataContext = value;
    }

    private bool _dragging;
    private Point _start;
    private Canvas? _parent;

    public SmartView()
    {
        InitializeComponent();
        // DataContext = this;

        this.PointerPressed += OnPointerPressed;
        this.PointerMoved += OnPointerMoved;
        this.PointerReleased += OnPointerReleased;
        this.PointerCaptureLost += (_, _) => _dragging = false;

        ResizeThumb.DragDelta += OnThumbResize;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _dragging = true;
            _start = e.GetPosition(this);
            _parent = this.GetVisualParent<Canvas>();
            e.Pointer.Capture(this);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_dragging || _parent is null)
            return;

        Point pos = e.GetPosition(_parent);
        Canvas.SetLeft(this, pos.X - _start.X);
        Canvas.SetTop(this, pos.Y - _start.Y);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragging = false;
        e.Pointer.Capture(null);

        if (_parent is SmartViewHost host)
            host.BringToFront(this);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        if (this.GetVisualParent() is SmartViewHost host)
            host.Children.Remove(this);
    }

    private void OnThumbResize(object? sender, VectorEventArgs e)
    {
        if (double.IsNaN(Width)) Width = Bounds.Width;
        if (double.IsNaN(Height)) Height = Bounds.Height;

        Width = Math.Clamp(Width + e.Vector.X, MinWidth, MaxWidth);
        Height = Math.Clamp(Height + e.Vector.Y, MinHeight, MaxHeight);

        Debug.Log(Height.ToString());
    }
}
