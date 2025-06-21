using Avalonia;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;
using System;
using System.Reactive.Linq;
using Utils.Converters;

namespace Client_PSL.Controls;

/// <summary>
/// Represents a user control that displays a color field and allows the user to select a color.
/// </summary>
/// <remarks>
/// The <see cref="ColorField"/> control provides a visual representation of a color and enables users to open a color picker popup to change the selected color.
/// It supports data binding for the selected color and can be integrated into forms or other UI elements where color selection is required.
/// </remarks>
public class ColorField : UserControl
{
    /// <summary>
    /// Identifies the <see cref="SelectedColor"/> dependency property.
    /// </summary>
    /// <remarks>
    /// This property enables data binding and change notification for the selected color in the color field control.
    /// The default value is <see cref="Colors.White"/>.
    /// </remarks>
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorField, Color>(nameof(SelectedColor), defaultValue: Colors.White);

    /// <summary>
    /// Gets or sets the currently selected color in the color field.
    /// </summary>
    /// <remarks>
    /// This property is bound to the color picker popup and updates whenever the user selects a new color.
    /// The default value is <see cref="Colors.White"/>.
    /// </remarks>
    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="DetailsVisible"/> dependency property.
    /// </summary>
    /// <remarks>
    /// This property enables data binding and change notification for the visibility of the details panel in the color field control.
    /// The default value is <c>false</c>, meaning the details panel is visible by default.
    /// </remarks>
    public static readonly StyledProperty<bool> DetailsVisibleProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(DetailsVisible), defaultValue: false);

    /// <summary>
    /// Gets or sets a value indicating whether the details panel is visible in the color field control.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, the details panel (containing additional color information or controls) is shown.
    /// When set to <c>false</c>, the details panel is hidden. The default value is <c>true</c>.
    /// </remarks>
    public bool DetailsVisible
    {
        get => GetValue(DetailsVisibleProperty);
        set => SetValue(DetailsVisibleProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorField"/> control.
    /// </summary>
    /// <remarks>
    /// The constructor sets up the visual layout and bindings for the color field, including the color display and the popup color picker.
    /// It also initializes the default values for the selected color and details panel visibility.
    /// </remarks>
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
