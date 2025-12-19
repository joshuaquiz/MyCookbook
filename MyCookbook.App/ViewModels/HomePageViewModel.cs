using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MyCookbook.App.Helpers;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;

namespace MyCookbook.App.ViewModels;

public partial class HomePageViewModel : BaseViewModel, IDisposable
{
    private readonly IRecipeService _recipeService;
    private readonly ISearchService _searchService;
    private readonly INotificationService _notificationService;
    private readonly System.Timers.Timer _searchTimer;
    private readonly int _pageSize = 20;
    private bool _disposed;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _includeIngredients = string.Empty;

    [ObservableProperty]
    private string _excludeIngredients = string.Empty;

    [ObservableProperty]
    private bool _isFilterVisible;

    [ObservableProperty]
    private Func<int, CancellationToken, IAsyncEnumerable<Components.RecipeSummary.RecipeSummaryViewModel>>? _getData;

    // This will be set by the view to trigger refresh
    public Action? TriggerRefresh { get; set; }

    public HomePageViewModel(
        IRecipeService recipeService,
        ISearchService searchService,
        INotificationService notificationService)
    {
        Debug.WriteLine("[HomePageViewModel] Constructor called");
        _recipeService = recipeService;
        _searchService = searchService;
        _notificationService = notificationService;
        _searchTimer = new System.Timers.Timer(500); // 0.5 seconds
        _searchTimer.Elapsed += OnSearchTimerElapsed;
        _searchTimer.AutoReset = false;
    }

    private void OnSearchTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        TriggerRefresh?.Invoke();
    }

    partial void OnGetDataChanged(Func<int, CancellationToken, IAsyncEnumerable<Components.RecipeSummary.RecipeSummaryViewModel>>? value)
    {
        Debug.WriteLine($"[HomePageViewModel] GetData property set: {value != null}");
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    partial void OnIncludeIngredientsChanged(string value)
    {
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    partial void OnExcludeIngredientsChanged(string value)
    {
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        IncludeIngredients = string.Empty;
        ExcludeIngredients = string.Empty;
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    [RelayCommand]
    private void ToggleFilter()
    {
        IsFilterVisible = !IsFilterVisible;
    }

    [RelayCommand]
    private void CategorySelected()
    {
        // Handle category selection
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    public async IAsyncEnumerable<Components.RecipeSummary.RecipeSummaryViewModel> GetRecipeData(
        int pageNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Debug.WriteLine($"[HomePageViewModel] GetRecipeData called - Page: {pageNumber}");

        List<Common.ApiModels.RecipeSummaryViewModel> result;

        try
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                result = await _searchService.GlobalSearchAsync(
                    SearchText,
                    string.Empty,
                    IncludeIngredients,
                    ExcludeIngredients,
                    _pageSize,
                    pageNumber * _pageSize,
                    cancellationToken);
            }
            else
            {
                result = await _recipeService.GetPopularRecipesAsync(
                    _pageSize,
                    _pageSize * pageNumber,
                    cancellationToken);
            }

            Debug.WriteLine($"[HomePageViewModel] GetRecipeData received {result.Count} items");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HomePageViewModel] Error loading recipes: {ex.Message}");

            // Show user-friendly error message
            var message = ErrorMessageHelper.GetUserFriendlyMessage(ex);
            await _notificationService.ShowErrorAsync(message, "Failed to Load Recipes");

            // Return empty enumerable instead of crashing
            yield break;
        }

        // Yield results outside of try-catch
        foreach (var item in result)
        {
            yield return new Components.RecipeSummary.RecipeSummaryViewModel
            {
                Guid = item.Guid,
                ImageUrlsRaw = item.ImageUrlsRaw,
                Name = item.Name,
                AuthorImageUrlRaw = item.AuthorImageUrlRaw,
                AuthorName = item.AuthorName,
                TotalMinutes = item.TotalMinutes,
                PrepMinutes = item.PrepMinutes,
                ItemUrlRaw = item.ItemUrlRaw,
                Servings = item.Servings,
                Difficulty = item.Difficulty,
                Category = item.Category ?? string.Empty,
                Calories = item.Calories,
                Hearts = item.Hearts,
                Rating = item.Rating,
                Tags = item.Tags ?? string.Empty
            };
        }

        Debug.WriteLine($"[HomePageViewModel] GetRecipeData finished yielding items");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed resources
            if (_searchTimer != null)
            {
                _searchTimer.Elapsed -= OnSearchTimerElapsed;
                _searchTimer.Stop();
                _searchTimer.Dispose();
            }

            // Clear delegate references to prevent memory leaks
            TriggerRefresh = null;
            GetData = null;
        }

        _disposed = true;
    }
}

