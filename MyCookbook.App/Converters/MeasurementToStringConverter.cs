using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;
using MyCookbook.Common.Enums;

namespace MyCookbook.App.Converters;

public sealed class MeasurementToStringConverter : IValueConverter
{
    private static readonly Dictionary<Measurement, string> MeasurementMap = new()
    {
        { Measurement.Unit, string.Empty },
        { Measurement.Piece, "piece" },
        { Measurement.Slice, "slice" },
        { Measurement.Clove, "clove" },
        { Measurement.Bunch, "bunch" },
        { Measurement.Cup, "cup" },
        { Measurement.TableSpoon, "tbsp" },
        { Measurement.TeaSpoon, "tsp" },
        { Measurement.Ounce, "oz" },
        { Measurement.Fillet, "fillet" },
        { Measurement.Inch, "in" },
        { Measurement.Can, "can" }
    };

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture) =>
        value != null && MeasurementMap.TryGetValue(
            value is string s
                ? Enum.Parse<Measurement>(s)
            : (Measurement)value,
            out var str)
            ? str
            : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
