using Avalonia;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;
using System;
using System.Reactive.Linq;
using Utils;

namespace Client_PSL.Controls;

public class ColorField : UserControl
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorField, Color>(nameof(SelectedColor), defaultValue: Colors.White);

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly StyledProperty<bool> DetailsVisibleProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(DetailsVisible), defaultValue: true);

    public bool DetailsVisible
    {
        get => GetValue(DetailsVisibleProperty);
        set => SetValue(DetailsVisibleProperty, value);
    }

    public ColorField()
    {
        Border previewBox = new()
        {
            Width = 40,
            Height = 20,
            CornerRadius = new CornerRadius(5),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };

        ColorPicker colorPicker = new();
        colorPicker.Bind(ColorPicker.SelectedColorProperty, new Binding
        {
            Source = this,
            Path = nameof(SelectedColor),
            Mode = BindingMode.TwoWay
        });
        colorPicker.Bind(ColorPicker.DetailsVisibleProperty, new Binding
        {
            Source = this,
            Path = nameof(DetailsVisible),
            Mode = BindingMode.TwoWay
        });

        Popup popup = new()
        {
            PlacementTarget = previewBox,
            Placement = PlacementMode.Bottom,
            Child = colorPicker,
        };
        popup.KeyDown += (_, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Escape)
                popup.IsOpen = false;
        };

        previewBox.PointerPressed += (_, _) =>
        {
            popup.IsOpen = !popup.IsOpen;
            if (popup.IsOpen)
                popup.Child.Focus();
        };

        previewBox.Bind(Border.BackgroundProperty, new Binding
        {
            Source = this,
            Path = nameof(SelectedColor),
            Converter = new ColorToBrushConverter(),
        });

        Content = new Grid
        {
            Children =
            {
                previewBox,
                popup
            },
        };
    }
}
