using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MyCookbook.App.Implementations;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

[QueryProperty(nameof(Recipe), nameof(Recipe))]
[QueryProperty(nameof(Guid), nameof(Guid))]
public partial class RecipeViewModel(
    CookbookHttpClient httpClient)
    : BaseViewModel
{
    [ObservableProperty]
    private RecipeModel? _recipe;

    [ObservableProperty]
    private string? _guid;

    partial void OnGuidChanged(string? value)
    {
        GetRecipeCommand.Execute(null);
    }

    [RelayCommand]
    private async Task GetRecipe()
    {
        IsBusy = true;
        Recipe = await httpClient.Get<RecipeModel>(
            new Uri(
                $"/api/Recipe/{Guid}",
                UriKind.Absolute),
            CancellationToken.None);
        IsBusy = false;
    }
}