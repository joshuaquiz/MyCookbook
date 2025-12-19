using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCookbook.App.Helpers;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class MyCookbookViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly INotificationService _notificationService;
    private DateTime? _lastLoadTime;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private List<RecipeModel>? _recipes;

    [ObservableProperty]
    private ObservableCollection<RecipeModel>? _recipesToDisplay;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public MyCookbookViewModel(
        IRecipeService recipeService,
        INotificationService notificationService)
    {
        _recipeService = recipeService;
        _notificationService = notificationService;
        Title = "My Cookbook";

        // Initialize with empty collections immediately for responsive UI
        Recipes = [];
        RecipesToDisplay = [];
    }

    /// <summary>
    /// Call this method when the page appears to load data asynchronously
    /// </summary>
    public void InitializeAsync()
    {
        // Only reload if cache is expired or empty
        if (_lastLoadTime == null ||
            DateTime.UtcNow - _lastLoadTime > CacheExpiration ||
            Recipes?.Count == 0)
        {
            // Fire and forget - don't block the UI
            _ = GetRecipesCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task GetRecipes()
    {
        IsRefreshing = true;
        IsBusy = true;

        try
        {
            var data = await _recipeService.GetPersonalCookbookAsync();

            Recipes ??= [];
            Recipes.Clear();
            foreach (var recipe in data)
            {
                Recipes.Add(recipe);
            }

            RecipesToDisplay = new ObservableCollection<RecipeModel>(Recipes);

            // Update cache timestamp
            _lastLoadTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            // Initialize with empty collections to prevent null reference errors
            Recipes ??= [];
            RecipesToDisplay ??= [];

            // Show user-friendly error message
            var message = ErrorMessageHelper.GetUserFriendlyMessage(ex);
            await _notificationService.ShowErrorAsync(message, "Failed to Load Cookbook");
        }
        finally
        {
            IsRefreshing = false;
            IsBusy = false;
        }
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