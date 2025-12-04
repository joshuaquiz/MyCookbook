using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using MyCookbook.App.Implementations;
using MyCookbook.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyCookbook.App.ViewModels;

[QueryProperty(nameof(Recipe), nameof(Recipe))]
[QueryProperty(nameof(Guid), nameof(Guid))]
[QueryProperty(nameof(PreviewName), nameof(PreviewName))]
[QueryProperty(nameof(PreviewImageUrl), nameof(PreviewImageUrl))]
[QueryProperty(nameof(PreviewAuthorName), nameof(PreviewAuthorName))]
[QueryProperty(nameof(PreviewAuthorImageUrl), nameof(PreviewAuthorImageUrl))]
[QueryProperty(nameof(PreviewTotalMinutes), nameof(PreviewTotalMinutes))]
[QueryProperty(nameof(PreviewPrepMinutes), nameof(PreviewPrepMinutes))]
[QueryProperty(nameof(PreviewServings), nameof(PreviewServings))]
[QueryProperty(nameof(PreviewDifficulty), nameof(PreviewDifficulty))]
[QueryProperty(nameof(PreviewCategory), nameof(PreviewCategory))]
[QueryProperty(nameof(PreviewCalories), nameof(PreviewCalories))]
[QueryProperty(nameof(PreviewItemUrl), nameof(PreviewItemUrl))]
[QueryProperty(nameof(PreviewHearts), nameof(PreviewHearts))]
[QueryProperty(nameof(PreviewRating), nameof(PreviewRating))]
public partial class RecipeViewModel(
    CookbookHttpClient httpClient)
    : BaseViewModel
{
    [ObservableProperty]
    private RecipeModel? _recipe;

    [ObservableProperty]
    private Guid _recipeGuid;

    [ObservableProperty]
    private ObservableCollection<Uri> _imageUrls = [];

    [ObservableProperty]
    private Uri? _url;

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
    [NotifyPropertyChangedFor(nameof(HasPrep))]
    private ObservableCollection<StepViewModel> _prepSteps = [];

    [ObservableProperty]
    private ObservableCollection<StepViewModel> _cookingSteps = [];

    [ObservableProperty]
    private ObservableCollection<RecipeIngredientViewModel> _recipeIngredients = [];

    [ObservableProperty]
    private int _recipeHearts;

    [ObservableProperty]
    private decimal? _rating;

    [ObservableProperty]
    private bool _hasPrep;

    public TimeSpan? TotalTime =>
        !PrepTime.HasValue && !CookTime.HasValue
            ? null
            : (PrepTime ?? TimeSpan.Zero) + (CookTime ?? TimeSpan.Zero);

    [ObservableProperty]
    private string? _guid;

    [ObservableProperty]
    private decimal _servingsMultiplier = 1m;

    private int? _originalServings;

    // Preview properties from summary card (all optional except Guid)
    // All are strings because Shell navigation passes query parameters as strings
    [ObservableProperty]
    private string? _previewName;

    [ObservableProperty]
    private string? _previewImageUrl;

    [ObservableProperty]
    private string? _previewAuthorName;

    [ObservableProperty]
    private string? _previewAuthorImageUrl;

    [ObservableProperty]
    private string? _previewTotalMinutes;

    [ObservableProperty]
    private string? _previewPrepMinutes;

    [ObservableProperty]
    private string? _previewServings;

    [ObservableProperty]
    private string? _previewDifficulty;

    [ObservableProperty]
    private string? _previewCategory;

    [ObservableProperty]
    private string? _previewCalories;

    [ObservableProperty]
    private string? _previewItemUrl;

    [ObservableProperty]
    private string? _previewHearts;

    [ObservableProperty]
    private string? _previewRating;

    private bool _hasLoadedFullRecipe;
    private bool _hasPrePopulated;

    partial void OnRecipeChanged(RecipeModel? value)
    {
        if (value.HasValue)
        {
            var recipe = value.Value;
            RecipeGuid = recipe.Guid;

            // Update ImageUrls collection
            ImageUrls.Clear();
            if (recipe.ImageUrls?.Count > 0)
            {
                foreach (var imageUrl in recipe.ImageUrls)
                {
                    ImageUrls.Add(imageUrl);
                }
            }

            Url = recipe.Url;
            Name = recipe.Name;
            PrepTime = recipe.PrepTime;
            CookTime = recipe.CookTime;
            Servings = recipe.Servings;
            Description = recipe.Description;
            RecipeHearts = recipe.RecipeHearts;
            Rating = recipe.Rating;
            UserProfile = recipe.UserProfile.HasValue
                ? new UserProfileViewModel(recipe.UserProfile.Value)
                : null;

            PrepSteps.Clear();
            foreach (var step in recipe.PrepSteps.OrderBy(x => x.StepNumber))
            {
                PrepSteps.Add(new StepViewModel(step));
                HasPrep = true;
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
        // Set RecipeGuid for API calls
        if (!string.IsNullOrEmpty(value) && System.Guid.TryParse(value, out var guid))
        {
            RecipeGuid = guid;
        }

        // Don't pre-populate here - wait for all preview properties to be set
        // PrePopulateFromPreviewData will be called after the last preview property is set
    }

    public void PrePopulateFromPreviewData()
    {
        if (_hasPrePopulated || _hasLoadedFullRecipe)
        {
            return;
        }

        // Pre-populate with any available preview data
        if (!string.IsNullOrEmpty(PreviewName))
        {
            Name = PreviewName;
        }

        if (!string.IsNullOrEmpty(PreviewImageUrl))
        {
            // Update ImageUrls collection for preview
            ImageUrls.Clear();

            // Try to parse as JSON array first (multiple URLs)
            try
            {
                var urls = JsonSerializer.Deserialize<List<string>>(PreviewImageUrl);
                if (urls != null)
                {
                    foreach (var url in urls)
                    {
                        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                        {
                            ImageUrls.Add(uri);
                        }
                    }
                }
            }
            catch
            {
                // If JSON parsing fails, treat as single URL (backward compatibility)
                if (Uri.TryCreate(PreviewImageUrl, UriKind.Absolute, out var imageUri))
                {
                    ImageUrls.Add(imageUri);
                }
            }
        }

        if (!string.IsNullOrEmpty(PreviewItemUrl) && Uri.TryCreate(PreviewItemUrl, UriKind.Absolute, out var itemUri))
        {
            Url = itemUri;
        }

        if (!string.IsNullOrEmpty(PreviewAuthorName))
        {
            // Create a basic UserProfile with just the name and image
            Uri? authorImageUri = null;
            if (!string.IsNullOrEmpty(PreviewAuthorImageUrl) && Uri.TryCreate(PreviewAuthorImageUrl, UriKind.Absolute, out var tempUri))
            {
                authorImageUri = tempUri;
            }

            // Create a minimal UserProfileModel for preview
            var previewUserProfile = new UserProfileModel(
                Guid: System.Guid.Empty,
                BackgroundImageUri: null,
                ProfileImageUri: authorImageUri,
                FirstName: PreviewAuthorName,
                LastName: string.Empty,
                Country: string.Empty,
                City: string.Empty,
                Age: 0,
                RecipesAdded: 0,
                Description: null,
                IsPremium: false,
                IsFollowed: false,
                RecentRecipes: []);

            UserProfile = new UserProfileViewModel(previewUserProfile);
        }

        // Parse numeric preview values from strings
        if (!string.IsNullOrEmpty(PreviewPrepMinutes) && long.TryParse(PreviewPrepMinutes, out var prepMinutes))
        {
            PrepTime = TimeSpan.FromMinutes(prepMinutes);
        }

        if (!string.IsNullOrEmpty(PreviewTotalMinutes) && long.TryParse(PreviewTotalMinutes, out var totalMinutes))
        {
            var totalTime = TimeSpan.FromMinutes(totalMinutes);
            if (PrepTime.HasValue)
            {
                CookTime = totalTime - PrepTime.Value;
            }
            else
            {
                // If we only have total time, assume it's all cook time
                CookTime = totalTime;
            }
        }

        if (!string.IsNullOrEmpty(PreviewServings) && int.TryParse(PreviewServings, out var servings))
        {
            Servings = servings;
            _originalServings = servings;
        }

        if (!string.IsNullOrEmpty(PreviewHearts) && int.TryParse(PreviewHearts, out var hearts))
        {
            RecipeHearts = hearts;
        }

        if (!string.IsNullOrEmpty(PreviewRating) && decimal.TryParse(PreviewRating, out var rating))
        {
            Rating = rating;
        }

        _hasPrePopulated = true;
        _ = GetRecipeCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task GetRecipe()
    {
        IsBusy = true;
        try
        {
            Recipe = await httpClient.Get<RecipeModel>(
                new Uri(
                    $"/api/Recipe/{Guid}",
                    UriKind.Absolute),
                CancellationToken.None);
            _hasLoadedFullRecipe = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Share()
    {
        if (Url == null || string.IsNullOrEmpty(Name))
        {
            return;
        }

        await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default
            .RequestAsync(
                new ShareTextRequest
                {
                    Uri = Url.AbsoluteUri,
                    Title = Name,
                    Text = $"Check out this recipe: {Name}"
                });
    }
}
