using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;
using System;
using System.Reactive.Linq;
using Utils.Converters;

namespace Client_PSL.Controls;

/// <summary>
/// Initializes a new instance of the <see cref="ColorPicker"/> control.
/// Sets up the color wheel, RGBA/Hex/Hue input fields, preview, and detail panel with data bindings.
/// Also handles keyboard shortcuts for toggling the details panel.
/// </summary>
/// <remarks>
/// The constructor creates and arranges all UI elements for the color picker, including:
/// <list type="bullet">
/// <item>
/// <description>
/// A <see cref="ColorWheelControl"/> for graphical color selection, bound bidirectionally to <see cref="SelectedColor"/>.
/// </description>
/// </item>
/// <item>
/// <description>
/// TextBoxes for RGBA, Hex, and Hue values, each bound to the corresponding property of the color wheel.
/// </description>
/// </item>
/// <item>
/// <description>
/// A slider for adjusting the alpha (transparency) channel.
/// </description>
/// </item>
/// <item>
/// <description>
/// A preview area showing the currently selected color.
/// </description>
/// </item>
/// <item>
/// <description>
/// A details panel that can be toggled with F1, showing or hiding the input fields and preview.
/// </description>
/// </item>
/// </list>
/// All bindings are set up for two-way synchronization where appropriate.
/// </remarks>
public class ColorPicker : UserControl
{
    /// <summary>
    /// Identifies the <see cref="SelectedColor"/> dependency property.
    /// </summary>
    /// <remarks>
    /// This property allows for data binding and change notification of the selected color within the color picker control.
    /// The default value is <see cref="Colors.White"/>.
    /// </remarks>
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorPicker, Color>(nameof(SelectedColor), defaultValue: Colors.White);

    /// <summary>
    /// Gets or sets the currently selected color in the color picker.
    /// </summary>
    /// <remarks>
    /// This property is bound to the <see cref="ColorWheelControl"/> and updates whenever the user selects a new color.
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
    /// This property enables data binding and change notification for the visibility of the details panel in the color picker control.
    /// The default value is <c>true</c>, meaning the details panel is visible by default.
    /// </remarks>
    public static readonly StyledProperty<bool> DetailsVisibleProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(DetailsVisible), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the details panel is visible in the color picker.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, the details panel (containing input fields and color preview) is shown.
    /// When set to <c>false</c>, the details panel is hidden. The default value is <c>true</c>.
    /// </remarks>
    public bool DetailsVisible
    {
        get => GetValue(DetailsVisibleProperty);
        set => SetValue(DetailsVisibleProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="DetailsVisible"/> dependency property.
    /// </summary>
    /// <remarks>
    /// This property enables data binding and change notification for the visibility of the details panel in the color picker control.
    /// The default value is <c>true</c>, meaning the details panel is visible by default.
    /// </remarks>
    public ColorPicker()
    {
        this.Focusable = true;
        this.KeyDown += (_, e) =>
        {
            if (e.Key == Avalonia.Input.Key.F1)
                DetailsVisible = !DetailsVisible;
        };

        ColorWheelControl wheel = new() { Height = 300, Width = 300 };
        wheel.Bind(ColorWheelControl.SelectedColorProperty, new Binding
        {
            Source = this,
            Path = nameof(SelectedColor),
            Mode = BindingMode.TwoWay
        });

        TextBox rBox = new() { Width = 40 };
        rBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.R),
            Mode = BindingMode.TwoWay,
            Converter = ByteStringConverter.Instance
        });

        TextBox gBox = new() { Width = 40 };
        gBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.G),
            Mode = BindingMode.TwoWay,
            Converter = ByteStringConverter.Instance
        });

        TextBox bBox = new() { Width = 40 };
        bBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.B),
            Mode = BindingMode.TwoWay,
            Converter = ByteStringConverter.Instance
        });

        TextBox aBox = new() { Width = 40 };
        aBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.A),
            Mode = BindingMode.TwoWay,
            Converter = ByteStringConverter.Instance
        });

        Slider alphaSlider = new() { Minimum = 0, Maximum = 255, Width = 120 };
        alphaSlider.Bind(Slider.ValueProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.A),
            Mode = BindingMode.TwoWay
        });

        TextBox hexBox = new() { Width = 120, HorizontalAlignment = HorizontalAlignment.Left };
        hexBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.Hex),
            Mode = BindingMode.TwoWay,
        });

        TextBox hueBox = new() { Width = 90, HorizontalAlignment = HorizontalAlignment.Left };
        hueBox.Bind(TextBox.TextProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.Hue),
            Mode = BindingMode.TwoWay,
        });

        Border preview = new()
        {
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            CornerRadius = new CornerRadius(5),
        };
        preview.Bind(Border.BackgroundProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.SelectedColor),
            Mode = BindingMode.OneWay,
            Converter = new ColorToBrushConverter()
        });

        StackPanel detailPanel = new()
        {
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Top,
            Children = {
                new TextBlock { Text = "RGBA:" },
                new StackPanel {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4,
                    Children = { rBox, gBox, bBox, aBox },
                },
                new TextBlock { Text = "Alpha:" },
                alphaSlider,
                new TextBlock { Text = "Hex:" },
                hexBox,
                new TextBlock { Text = "Hue:" },
                hueBox,
                new TextBlock { Text = "Preview:" },
                preview,
            }
        };
        detailPanel.Bind(IsVisibleProperty, new Binding
        {
            Source = this,
            Path = nameof(DetailsVisible),
            Mode = BindingMode.OneWay
        });

        Border mainBorder = new()
        {
            Background = Brushes.Black,
            BorderThickness = new Thickness(3),
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(5),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Margin = new Thickness(12),
                Children = { wheel, detailPanel },
            }
        };
        mainBorder.Bind(Border.BorderBrushProperty, new Binding
        {
            Source = wheel,
            Path = nameof(wheel.SelectedColor),
            Mode = BindingMode.OneWay,
            Converter = new ColorToBrushConverter()
        });

        Content = mainBorder;
    }
}
