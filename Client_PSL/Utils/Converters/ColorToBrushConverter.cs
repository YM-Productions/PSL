using Avalonia;
using Avalonia.Media;
using Avalonia.Data.Converters;
using System;

namespace Utils.Converters;

/// <summary>
/// Converts between <see cref="Color"/> values and <see cref="SolidColorBrush"/> objects for use in data binding scenarios.
/// </summary>
/// <remarks>
/// This converter is typically used in UI frameworks such as Avalonia to allow binding a <see cref="Color"/> property to a UI element that expects a <see cref="IBrush"/>.
/// When converting, it creates a new <see cref="SolidColorBrush"/> from the provided <see cref="Color"/>.
/// The <c>ConvertBack</c> method is not supported and will throw a <see cref="NotSupportedException"/>.
/// </remarks>
public class ColorToBrushConverter : IValueConverter
{
    /// <summary>
    /// A singleton instance of the <see cref="ColorToBrushConverter"/> for reuse.
    /// </summary>
    public static readonly ColorToBrushConverter Instance = new();

    /// <summary>
    /// Converts a <see cref="Color"/> value to a <see cref="SolidColorBrush"/>.
    /// </summary>
    /// <param name="value">The value produced by the binding source. Expected to be a <see cref="Color"/>.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic (not used).</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A <see cref="SolidColorBrush"/> created from the input <see cref="Color"/>, or <see cref="AvaloniaProperty.UnsetValue"/> if the input is not a <see cref="Color"/>.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Color color)
            return new SolidColorBrush(color);
        return AvaloniaProperty.UnsetValue;
    }

    /// <summary>
    /// Not supported. This converter does not support converting a brush back to a color.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target (not used).</param>
    /// <param name="targetType">The type to convert to (not used).</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns>
    /// This method always throws a <see cref="NotSupportedException"/>.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
