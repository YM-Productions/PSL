using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Utils;
using CLient_PSL.Style.Animations;

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

    public static readonly StyledProperty<object?> InnerContentProperty =
        AvaloniaProperty.Register<SmartView, object?>(nameof(InnerContent));

    public object? InnerContent
    {
        get => GetValue(InnerContentProperty);
        set => SetValue(InnerContentProperty, value);
    }

    private bool _dragging;
    private Point _start;
    private Canvas? _parent;

    private const double SnapThreshold = 15.0;

    private bool _minimzed = false;
    private bool _snappedToX = false;
    private bool _snappedToY = false;
    private double _snapX = 0;
    private double _snapY = 0;

    private double _originalMinHeight;
    private double _originalHeight;

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
        if (e.Source != HeaderBorder)
            return;

        if (this.GetVisualParent() is SmartViewHost host)
            host.BringToFront(this);

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
        if (!_dragging || _parent is not SmartViewHost host)
            return;

        Point pos = e.GetPosition(host);
        double x = pos.X - _start.X;
        double y = pos.Y - _start.Y;
        double unsnappedX = x;
        double unsnappedY = y;

        //Window Clamping
        x = Math.Clamp(x, 0, Math.Max(0, host.Bounds.Width - Bounds.Width));
        y = Math.Clamp(y, 0, Math.Max(0, host.Bounds.Height - Bounds.Height));

        // Window snapping
        KeyModifiers modifiers = e.KeyModifiers;
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            if (Math.Abs(x) < SnapThreshold)
                x = 0;
            if (Math.Abs(y) < SnapThreshold)
                y = 0;

            double rightEdge = x + Bounds.Width;
            double bottomEdge = y + Bounds.Height;

            if (Math.Abs(rightEdge - host.Bounds.Width) < SnapThreshold)
                x = host.Bounds.Width - Bounds.Width;
            if (Math.Abs(bottomEdge - host.Bounds.Height) < SnapThreshold)
                y = host.Bounds.Height - Bounds.Height;

            foreach (SmartView other in host.Children)
            {
                if (other == this)
                    continue;

                double ox = Canvas.GetLeft(other);
                double oy = Canvas.GetTop(other);
                double ow = other.Bounds.Width;
                double oh = other.Bounds.Height;

                if (Math.Abs(x - (ox + ow)) < SnapThreshold)
                    x = ox + ow;
                if (Math.Abs((x + Bounds.Width) - ox) < SnapThreshold)
                    x = ox - Bounds.Width;
                if (Math.Abs(y - (oy + oh)) < SnapThreshold)
                    y = oy + oh;
                if (Math.Abs((y + Bounds.Height) - oy) < SnapThreshold)
                    y = oy - Bounds.Height;
            }
        }

        bool snappEffect = false;
        if (x != unsnappedX && !_snappedToX)
        {
            _snappedToX = true;
            _snapX = x;
            _ = UIEffects.PlaySnapEffect(this);
            snappEffect = true;
        }
        if (y != unsnappedY && !_snappedToY)
        {
            _snappedToY = true;
            _snapY = y;
            if (!snappEffect)
                _ = UIEffects.PlaySnapEffect(this);
        }

        if (_snappedToX && _snapX != x)
            _snappedToX = false;
        if (_snappedToY && _snapY != y)
            _snappedToY = false;

        Canvas.SetLeft(this, x);
        Canvas.SetTop(this, y);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragging)
        {
            _dragging = false;
            e.Pointer.Capture(null);

            if (_parent is SmartViewHost host)
                host.BringToFront(this);
        }
    }

    private void OnMinimizeButtonClick(object? sender, RoutedEventArgs e)
    {
        _minimzed = !_minimzed;

        if (_minimzed)
        {
            _originalMinHeight = MinHeight;
            _originalHeight = Height;
        }

        ContentHost.IsVisible = !_minimzed;
        ResizeThumb.IsVisible = !_minimzed;

        Height = _minimzed ? double.NaN : _originalHeight;
        MinHeight = _minimzed ? 0 : _originalMinHeight;
    }

    // private void OnMaximizeButtonClick(object? sender, RoutedEventArgs e)
    // {
    //
    // }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        if (this.GetVisualParent() is SmartViewHost host)
            host.Children.Remove(this);
    }

    private void OnThumbResize(object? sender, VectorEventArgs e)
    {
        if (_parent is null) _parent = this.GetVisualParent<Canvas>();
        if (_parent is not SmartViewHost host)
            return;

        if (double.IsNaN(Width)) Width = Bounds.Width;
        if (double.IsNaN(Height)) Height = Bounds.Height;

        double newWidth = Width + e.Vector.X;
        double newHeight = Height + e.Vector.Y;

        foreach (SmartView other in host.Children)
        {
            if (other == this)
                continue;

            double ox = Canvas.GetLeft(other);
            double oy = Canvas.GetTop(other);
            double ow = other.Bounds.Width;
            double oh = other.Bounds.Height;

            if (Math.Abs((Canvas.GetLeft(this) + newWidth) - ox) < SnapThreshold)
                newWidth = ox - Canvas.GetLeft(this);
            if (Math.Abs((Canvas.GetTop(this) + newHeight) - oy) < SnapThreshold)
                newHeight = oy - Canvas.GetTop(this);
        }

        double maxW = host.Bounds.Width - Canvas.GetLeft(this);
        double maxH = host.Bounds.Height - Canvas.GetTop(this);

        if (maxW < MinWidth) maxW = MinWidth;
        else if (maxW > MaxWidth) maxW = MaxWidth;
        if (maxH < MinHeight) maxH = MinHeight;
        else if (maxH > MaxHeight) maxH = MaxHeight;

        Width = Math.Clamp(newWidth, MinWidth, maxW);
        Height = Math.Clamp(newHeight, MinHeight, maxH);
    }
}
