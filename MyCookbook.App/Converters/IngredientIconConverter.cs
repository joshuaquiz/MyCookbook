using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using MyCookbook.App.Helpers;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Converters;

/// <summary>
/// Converter that returns an ingredient icon based on the ingredient name.
/// If the ingredient has an ImageUri, it returns that. Otherwise, it returns a matching icon.
/// </summary>
public class IngredientIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RecipeIngredientViewModel ingredient)
        {
            // If ingredient has an image URI, use it
            if (ingredient.ImageUri != null)
            {
                return ingredient.ImageUri;
            }

            // Otherwise, get the icon based on ingredient name
            var iconName = IngredientIconHelper.GetIconForIngredient(ingredient.Name);
            return iconName;
        }

        // Default fallback
        return "carrot";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

