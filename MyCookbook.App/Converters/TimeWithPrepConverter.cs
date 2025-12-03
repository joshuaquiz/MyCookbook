using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Converters;

public sealed class TimeWithPrepConverter : IMultiValueConverter
{
    public object Convert(
        object?[] values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not TimeSpan totalTime || values[1] is not TimeSpan prepTime)
        {
            return string.Empty;
        }

        var totalStr = FormatTimeSpan(totalTime);
        
        if (prepTime.TotalMinutes > 0)
        {
            var prepStr = FormatTimeSpan(prepTime);
            return $"{totalStr} ({prepStr} prep)";
        }

        return totalStr;
    }

    public object[] ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        var hours = (int)timeSpan.TotalHours;
        var minutes = timeSpan.Minutes;

        if (hours > 0)
        {
            return minutes > 0 ? $"{hours} h {minutes} mins" : $"{hours} h";
        }

        return $"{minutes} mins";
    }
}

