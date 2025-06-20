using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Utils;

public class ByteStringConverter : IValueConverter
{
    public static readonly ByteStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is byte b ? b.ToString() : "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return byte.TryParse(value?.ToString(), out var b) ? b : (byte)0;
    }
}
