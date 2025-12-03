using System;
using Microsoft.Maui.Controls;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class RecipePage
{
    private RecipeViewModel ViewModel { get; set; }

    public RecipePage(
        RecipeViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;

        // Subscribe to Loaded event to load data after page is fully rendered
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        // Unsubscribe to prevent multiple calls
        Loaded -= OnPageLoaded;

        // Load the full recipe data - the method itself handles async execution
        ViewModel.PrePopulateFromPreviewData();
    }

    private void ServingsPicker_SelectedIndexChanged(
        object sender,
        EventArgs e)
    {
        if (sender is Picker { SelectedItem: string selectedValue }
            && int.TryParse(selectedValue, out var newServings))
        {
            ViewModel.UpdateServingsMultiplier(newServings);
        }
    }
}
