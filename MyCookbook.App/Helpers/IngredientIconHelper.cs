using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCookbook.App.Helpers;

public static class IngredientIconHelper
{
    private static readonly Dictionary<string, string> IngredientKeywords = new()
    {
        // Liquids
        { "water", "water" },
        { "milk", "milk" },
        { "cream", "milk" },
        
        // Vegetables
        { "carrot", "carrot" },
        { "garlic", "garlic" },
        { "onion", "onion" },
        { "tomato", "tomato" },
        { "potato", "potato" },
        { "pepper", "pepper" },
        { "bell pepper", "pepper" },
        
        // Proteins
        { "chicken", "chicken" },
        { "beef", "beef" },
        { "pork", "pork" },
        { "steak", "steak" },
        { "fish", "fish" },
        { "salmon", "fish" },
        { "tuna", "fish" },
        { "egg", "egg" },
        
        // Dairy
        { "cheese", "cheese" },
        { "cheddar", "cheese" },
        { "mozzarella", "cheese" },
        { "parmesan", "cheese" },
        
        // Grains & Bread
        { "bread", "bread" },
        { "toast", "bread" },
        { "bun", "bread" },
        { "roll", "bread" },
        
        // Seasonings
        { "salt", "salt" },
        
        // Fruits
        { "apple", "apple" },
    };

    /// <summary>
    /// Gets the icon name for an ingredient based on its name.
    /// Returns the icon name if a match is found, otherwise returns "carrot" as default.
    /// </summary>
    public static string GetIconForIngredient(string? ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            return "carrot";
        }

        var lowerName = ingredientName.ToLowerInvariant();

        // Try exact match first
        if (IngredientKeywords.TryGetValue(lowerName, out var exactIcon))
        {
            return exactIcon;
        }

        // Try partial match (check if ingredient name contains any keyword)
        var partialMatch = IngredientKeywords
            .FirstOrDefault(kvp => lowerName.Contains(kvp.Key));

        if (!string.IsNullOrEmpty(partialMatch.Key))
        {
            return partialMatch.Value;
        }

        // Default fallback
        return "carrot";
    }
}

