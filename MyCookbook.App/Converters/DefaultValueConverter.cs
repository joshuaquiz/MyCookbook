using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Converters;

public sealed class DefaultValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case TimeSpan t:
            {
                var min = parameter == null
                    ? TimeSpan.Zero
                    : TimeSpan.FromSeconds(int.Parse((string)parameter));
                return t <= min;
            }
            case int i:
            {
                var min = parameter == null
                    ? 0
                    : int.Parse((string)parameter);
                return i <= min;
            }
        }

        if (!targetType.IsValueType)
        {
            return value == null;
        }

        return value == Activator.CreateInstance(targetType);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}