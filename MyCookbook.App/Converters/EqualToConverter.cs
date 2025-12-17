using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Converters;

public sealed class EqualToConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return false;
        }

        // Try to convert both to the same type for comparison
        if (int.TryParse(value.ToString(), out var intValue) && 
            int.TryParse(parameter.ToString(), out var intParameter))
        {
            return intValue == intParameter;
        }

        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

