using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Utils.Converters;

/// <summary>
/// Converts between <see cref="byte"/> values and their string representations for use in data binding scenarios.
/// </summary>
/// <remarks>
/// This converter is typically used in UI frameworks (such as Avalonia or WPF) to display byte values as strings in the user interface,
/// and to parse user input strings back into byte values. If the input value is not a valid byte, the converter returns 0.
/// </remarks>
public class ByteStringConverter : IValueConverter
{
    /// <summary>
    /// A singleton instance of the <see cref="ByteStringConverter"/> for reuse.
    /// </summary>
    public static readonly ByteStringConverter Instance = new();

    /// <summary>
    /// Converts a <see cref="byte"/> value to its string representation.
    /// </summary>
    /// <param name="value">The value produced by the binding source. Expected to be a <see cref="byte"/>.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic (not used).</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A string representation of the byte value, or "0" if the input is not a byte.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is byte b ? b.ToString() : "0";
    }

    /// <summary>
    /// Converts a string representation of a byte back to a <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target. Expected to be a string.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic (not used).</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// The parsed <see cref="byte"/> value, or 0 if parsing fails.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return byte.TryParse(value?.ToString(), out var b) ? b : (byte)0;
    }
}
