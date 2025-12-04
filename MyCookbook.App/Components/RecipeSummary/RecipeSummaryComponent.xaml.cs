using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyCookbook.App.Views;

namespace MyCookbook.App.Components.RecipeSummary;

public partial class RecipeSummaryComponent
{
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item),
        typeof(RecipeSummaryViewModel),
        typeof(RecipeSummaryComponent));

    public RecipeSummaryViewModel Item
    {
        get => (RecipeSummaryViewModel)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public RecipeSummaryComponent()
    {
        InitializeComponent();
        BindingContext = Item;
    }

    private async void OnTapped(
        object? sender,
        TappedEventArgs e)
    {
        if (sender is not Border border)
        {
            return;
        }

        try
        {
            // Trigger pressed visual state for immediate feedback
            VisualStateManager.GoToState(border, "Pressed");

            // Small delay to show the pressed state
            await Task.Delay(100);

            await NavigateToRecipeDetails();

            // Reset to normal state immediately
            VisualStateManager.GoToState(border, "Normal");
        }
        catch (Exception ex)
        {
            // An unexpected error occurred. No browser may be installed on the device.
            Console.WriteLine(ex.Message);

            // Make sure to reset state even on error
            VisualStateManager.GoToState(border, "Normal");
        }
    }

    private async void OnImageTapped(
        object? sender,
        TappedEventArgs e)
    {
        try
        {
            await NavigateToRecipeDetails();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private Task NavigateToRecipeDetails()
    {
        // Build navigation URL with preview data for immediate display
        var navigationUrl = $"{nameof(RecipePage)}?{nameof(Guid)}={Item.Guid}";

        // Add preview data if available (all optional except Guid)
        if (!string.IsNullOrEmpty(Item.Name))
        {
            navigationUrl += $"&PreviewName={Uri.EscapeDataString(Item.Name)}";
        }

        if (Item.ImageUrls?.Count > 0)
        {
            // Serialize all image URLs as JSON array
            var imageUrlsJson = JsonSerializer.Serialize(Item.ImageUrls.Select(u => u.AbsoluteUri).ToList());
            navigationUrl += $"&PreviewImageUrl={Uri.EscapeDataString(imageUrlsJson)}";
        }

        if (!string.IsNullOrEmpty(Item.AuthorName))
        {
            navigationUrl += $"&PreviewAuthorName={Uri.EscapeDataString(Item.AuthorName)}";
        }

        if (Item.AuthorImageUrl != null)
        {
            navigationUrl += $"&PreviewAuthorImageUrl={Uri.EscapeDataString(Item.AuthorImageUrl.AbsoluteUri)}";
        }

        if (Item.TotalMinutes > 0)
        {
            navigationUrl += $"&PreviewTotalMinutes={Item.TotalMinutes}";
        }

        if (Item.PrepMinutes > 0)
        {
            navigationUrl += $"&PreviewPrepMinutes={Item.PrepMinutes}";
        }

        if (Item.Servings.HasValue)
        {
            navigationUrl += $"&PreviewServings={Item.Servings.Value}";
        }

        if (!string.IsNullOrEmpty(Item.Difficulty))
        {
            navigationUrl += $"&PreviewDifficulty={Uri.EscapeDataString(Item.Difficulty)}";
        }

        if (!string.IsNullOrEmpty(Item.Category))
        {
            navigationUrl += $"&PreviewCategory={Uri.EscapeDataString(Item.Category)}";
        }

        if (Item.Calories.HasValue)
        {
            navigationUrl += $"&PreviewCalories={Item.Calories.Value}";
        }

        if (Item.ItemUrl != null)
        {
            navigationUrl += $"&PreviewItemUrl={Uri.EscapeDataString(Item.ItemUrl.AbsoluteUri)}";
        }

        // Always pass hearts (even if 0) to show the correct count
        navigationUrl += $"&PreviewHearts={Item.Hearts}";

        if (Item.Rating.HasValue)
        {
            navigationUrl += $"&PreviewRating={Item.Rating.Value}";
        }

        // Navigate to recipe page with preview data (fire and forget for immediate navigation)
        return Shell.Current.GoToAsync(navigationUrl);
    }
}