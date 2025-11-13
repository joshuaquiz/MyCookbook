using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;
using MyCookbook.Common.Enums;

namespace MyCookbook.App.Converters;

public sealed class MeasurementToStringConverter : IValueConverter
{
    private static readonly Dictionary<MeasurementUnit, string> MeasurementMap = new()
    {
        { MeasurementUnit.Unit, string.Empty },
        { MeasurementUnit.Piece, "piece" },
        { MeasurementUnit.Slice, "slice" },
        { MeasurementUnit.Clove, "clove" },
        { MeasurementUnit.Bunch, "bunch" },
        { MeasurementUnit.Cup, "cup" },
        { MeasurementUnit.TableSpoon, "tbsp" },
        { MeasurementUnit.TeaSpoon, "tsp" },
        { MeasurementUnit.Ounce, "oz" },
        { MeasurementUnit.Fillet, "fillet" },
        { MeasurementUnit.Inch, "in" },
        { MeasurementUnit.Can, "can" }
    };

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture) =>
        value != null && MeasurementMap.TryGetValue(
            value is string s
                ? Enum.Parse<MeasurementUnit>(s)
            : (MeasurementUnit)value,
            out var str)
            ? str
            : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
