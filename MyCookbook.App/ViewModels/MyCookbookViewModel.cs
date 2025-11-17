using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCookbook.App.Implementations;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class MyCookbookViewModel : BaseViewModel
{
    private readonly CookbookHttpClient _httpClient;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private List<RecipeModel>? _recipes;

    [ObservableProperty]
    private ObservableCollection<RecipeModel>? _recipesToDisplay;

    public MyCookbookViewModel(
        CookbookHttpClient httpClient)
    {
        _httpClient = httpClient;
        Title = "My Cookbook";
        GetRecipesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task GetRecipes()
    {
        IsRefreshing = true;
        IsBusy = true;
        var data = await _httpClient.GetFromJsonAsync<List<RecipeModel>>(
                       "/api/Personal/Cookbook")
                   ?? [];
        foreach (var recipe in data)
        {
            Recipes?.Add(recipe);
        }

        RecipesToDisplay = new ObservableCollection<RecipeModel>(Recipes ?? []);
        IsRefreshing = false;
        IsBusy = false;
    }

    [RelayCommand]
    private void Search(string text)
    {
        RecipesToDisplay?.Clear();
        if (string.IsNullOrWhiteSpace(text))
        {
            foreach (var recipe in Recipes ?? [])
            {
                RecipesToDisplay?.Add(recipe);
            }

            return;
        }

        var recipes = new HashSet<RecipeModel>();
        var terms = text
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();
        foreach (var recipe in terms
                     .SelectMany(
                         term =>
                             Recipes
                                 ?.Where(
                                     x =>
                                         x.Name.Contains(term))
                             ?? []))
        {
            recipes.Add(recipe);
        }

        foreach (var recipe in terms
                     .SelectMany(term =>
                         Recipes
                             ?.Where(
                                 x =>
                                     x.RecipeIngredients.Any(
                                         i =>
                                             i.Ingredient
                                                 .Name
                                                 .Contains(
                                                     term)))
                         ?? []))
        {
            recipes.Add(recipe);
        }

        terms = text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();
        foreach (var recipe in terms
                     .SelectMany(
                         term =>
                             Recipes
                                 ?.Where(
                                     x =>
                                         x.Name
                                             .Contains(
                                                 term))
                             ?? []))
        {
            recipes.Add(recipe);
        }

        foreach (var recipe in recipes)
        {
            RecipesToDisplay?.Add(recipe);
        }
    }
}