using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private Guid _recipeGuid;

    [ObservableProperty]
    private Uri? _image;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTime))]
    private TimeSpan? _prepTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTime))]
    private TimeSpan? _cookTime;

    [ObservableProperty]
    private int _servings;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private UserProfileViewModel? _userProfile;

    [ObservableProperty]
    private ObservableCollection<StepViewModel> _prepSteps = [];

    [ObservableProperty]
    private ObservableCollection<StepViewModel> _cookingSteps = [];

    [ObservableProperty]
    private ObservableCollection<RecipeIngredientViewModel> _recipeIngredients = [];

    public TimeSpan? TotalTime =>
        !PrepTime.HasValue && !CookTime.HasValue
            ? null
            : (PrepTime ?? TimeSpan.Zero) + (CookTime ?? TimeSpan.Zero);

    public bool HasPrep => PrepSteps.Count > 0;

    [ObservableProperty]
    private string? _guid;

    [ObservableProperty]
    private decimal _servingsMultiplier = 1m;

    private int? _originalServings;

    partial void OnRecipeChanged(RecipeModel? value)
    {
        if (value.HasValue)
        {
            var recipe = value.Value;
            RecipeGuid = recipe.Guid;
            Image = recipe.Image;
            Name = recipe.Name;
            PrepTime = recipe.PrepTime;
            CookTime = recipe.CookTime;
            Servings = recipe.Servings;
            Description = recipe.Description;
            UserProfile = recipe.UserProfile.HasValue 
                ? new UserProfileViewModel(recipe.UserProfile.Value) 
                : null;

            PrepSteps.Clear();
            foreach (var step in recipe.PrepSteps.OrderBy(x => x.StepNumber))
            {
                PrepSteps.Add(new StepViewModel(step));
            }

            CookingSteps.Clear();
            foreach (var step in recipe.CookingSteps.OrderBy(x => x.StepNumber))
            {
                CookingSteps.Add(new StepViewModel(step));
            }

            RecipeIngredients.Clear();
            foreach (var ingredient in recipe.RecipeIngredients)
            {
                RecipeIngredients.Add(new RecipeIngredientViewModel(ingredient));
            }

            _originalServings = recipe.Servings;
            ServingsMultiplier = 1m;
        }
    }

    public void UpdateServingsMultiplier(int newServings)
    {
        if (_originalServings is > 0)
        {
            ServingsMultiplier = (decimal)newServings / _originalServings.Value;
        }
    }

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
