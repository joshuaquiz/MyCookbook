using System;
using System.Collections.Generic;
using MyCookbook.Common.Enums;

namespace MyCookbook.App.Converters;

public static class QuantityToFractionConverter
{
    private static readonly Dictionary<string, string> FractionMap = new()
    {
        { "0.125", "⅛" },
        { "0.25", "¼" },
        { "0.33", "⅓" },
        { "0.333", "⅓" },
        { "0.375", "⅜" },
        { "0.5", "½" },
        { "0.625", "⅝" },
        { "0.67", "⅔" },
        { "0.667", "⅔" },
        { "0.75", "¾" },
        { "0.875", "⅞" }
    };

    private static readonly List<MeasurementUnit> MeasurementsToSkip =
    [
        MeasurementUnit.Inch
    ];

    public static object GetGeneratedQuantity(
        decimal quantity,
        decimal multiplier,
        MeasurementUnit? measurement)
    {
        if (measurement == null
            || !MeasurementsToSkip.Contains(measurement.Value))
        {

            quantity *= multiplier;
        }

        // Handle whole numbers
        if (quantity == Math.Floor(quantity))
        {
            return quantity.ToString("0");
        }

        // Split into whole and fractional parts
        var wholePart = (int)Math.Floor(quantity);
        var fractionalPart = quantity - wholePart;

        // Check for exact fraction matches on fractional part
        var fractionalStr = fractionalPart.ToString("0.###");
        if (FractionMap.TryGetValue(fractionalStr, out var fraction))
        {
            return wholePart > 0 ? $"{wholePart} {fraction}" : fraction;
        }

        // Check for close matches on fractional part (within 0.01)
        foreach (var kvp in FractionMap)
        {
            if (decimal.TryParse(kvp.Key, out var fractionValue) && 
                Math.Abs(fractionalPart - fractionValue) < 0.01m)
            {
                return wholePart > 0 ? $"{wholePart} {kvp.Value}" : kvp.Value;
            }
        }

        // Default to decimal with max 2 decimal places
        return quantity.ToString("0.##");
    }
}
