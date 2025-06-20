using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;
using System;
using System.Reactive.Linq;
using Utils;

namespace Client_PSL.Controls;

public class ColorPicker : UserControl
{
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorPicker, Color>(nameof(SelectedColor), defaultValue: Colors.White);

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public ColorPicker()
    {
        ColorWheelControl wheel = new() { Height = 300, Width = 300 };
        wheel[!ColorWheelControl.SelectedColorProperty] = this[!SelectedColorProperty];


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

        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(12),
            Children = {
                wheel,
                new StackPanel {
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
                }
            }
        };
    }
}
