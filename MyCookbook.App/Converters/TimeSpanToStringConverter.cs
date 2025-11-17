using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Converters;

public sealed class TimeSpanToStringConverter
    : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan)
        {
            return string.Empty;
        }

        var hours = (int)timeSpan.TotalHours;
        var minutes = timeSpan.Minutes;

        if (hours > 0)
        {
            return minutes > 0 ? $"{hours} h {minutes} mins" : $"{hours} h";
        }

        return $"{minutes} mins";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}