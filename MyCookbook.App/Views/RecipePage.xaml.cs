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
