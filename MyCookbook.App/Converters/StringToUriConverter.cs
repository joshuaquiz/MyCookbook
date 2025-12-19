using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Converters;

/// <summary>
/// Converts a string URL to a Uri object
/// </summary>
public class StringToUriConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
        {
            if (Uri.TryCreate(stringValue, UriKind.Absolute, out var uri))
            {
                return uri;
            }
        }
        
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }
}

