using Microsoft.Maui.Controls;
using MyCookbook.App.ViewModels;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyCookbook.App.Converters;

public sealed class IngredientToStringConverter : IMultiValueConverter
{
    private static readonly List<MeasurementUnit> MeasurementsToSkip =
    [
        MeasurementUnit.Inch
    ];

    private static readonly List<(decimal Value, string Fraction)> FractionValues = new()
    {
        (0.125m, "⅛"),
        (0.25m, "¼"),
        (0.33333m, "⅓"),
        (0.375m, "⅜"),
        (0.5m, "½"),
        (0.625m, "⅝"),
        (0.66667m, "⅔"),
        (0.75m, "¾"),
        (0.875m, "⅞")
    };

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
        { MeasurementUnit.Can, "can" },
        { MeasurementUnit.Pound, "lb" },
        { MeasurementUnit.Stick, "stick" }
    };

    public object Convert(
        object?[] values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not RecipeIngredientViewModel recipeIngredientModel || values[1] is not decimal multiplier)
        {
            return string.Empty; // Handle invalid inputs
        }

        string? quantity;
        var isSingular = false;
        switch (recipeIngredientModel.QuantityType!)
        {
            case QuantityType.Number when recipeIngredientModel.NumberValue.HasValue:
                quantity = GetGeneratedQuantity(recipeIngredientModel.NumberValue.Value, multiplier,
                    recipeIngredientModel.MeasurementUnit);
                isSingular = recipeIngredientModel.NumberValue.Value == 1;
                break;
            case QuantityType.Range when recipeIngredientModel is { MinValue: not null, MaxValue: not null }:
                quantity = $"{GetGeneratedQuantity(recipeIngredientModel.MinValue.Value, multiplier, recipeIngredientModel.MeasurementUnit)}-{GetGeneratedQuantity(recipeIngredientModel.MaxValue.Value, multiplier, recipeIngredientModel.MeasurementUnit)}";
                isSingular = false;
                break;
            case QuantityType.Min when recipeIngredientModel.MinValue.HasValue:
                quantity = $"{GetGeneratedQuantity(recipeIngredientModel.MinValue.Value, multiplier, recipeIngredientModel.MeasurementUnit)}+";
                isSingular = false;
                break;
            case QuantityType.Max when recipeIngredientModel.MaxValue.HasValue:
                quantity = $"Up to {GetGeneratedQuantity(recipeIngredientModel.MaxValue.Value, multiplier, recipeIngredientModel.MeasurementUnit)}";
                isSingular = false;
                break;
            case QuantityType.Unknown:
            default:
                quantity = string.Empty;
                break;
        }

        var unit = MeasurementMap.TryGetValue(
            recipeIngredientModel.MeasurementUnit,
            out var str)
            ? str
            : string.Empty;
        if (!isSingular)
        {
            unit += "s";
        }

        return $"{quantity} {unit} {recipeIngredientModel.Name}{(string.IsNullOrEmpty(recipeIngredientModel.Notes) ? string.Empty : ", " + recipeIngredientModel.Notes)}";
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string GetGeneratedQuantity(
        decimal quantity,
        decimal multiplier,
        MeasurementUnit? measurement)
    {
        if (measurement == null
            || !MeasurementsToSkip.Contains(measurement.Value))
        {
            quantity *= multiplier;
        }

        if (quantity == Math.Floor(quantity))
        {
            return quantity.ToString("0");
        }

        var wholePart = (int)Math.Floor(quantity);
        var fractionalPart = quantity - wholePart;
        var minDifference = 0.05m;
        string? closestFraction = null;
        foreach (var (fractionValue, fractionString) in FractionValues)
        {
            var difference = Math.Abs(fractionalPart - fractionValue);
            if (difference < minDifference)
            {
                minDifference = difference;
                closestFraction = fractionString;
            }
        }

        return closestFraction != null
            ? wholePart > 0 ? $"{wholePart} {closestFraction}" : closestFraction
            : quantity.ToString("0.##");
    }
}
