using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using MyCookbook.App.Services;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

[QueryProperty(nameof(RecipeId), nameof(RecipeId))]
[QueryProperty(nameof(RecipeUrl), nameof(RecipeUrl))]
public partial class ShareRecipeViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private CancellationTokenSource? _searchCancellationTokenSource;
    private System.Timers.Timer? _searchDebounceTimer;

    [ObservableProperty]
    private string _recipeId = string.Empty;

    [ObservableProperty]
    private string _recipeUrl = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ShareableAuthorViewModel> _shareableAuthors = [];

    [ObservableProperty]
    private ShareableAuthorViewModel? _selectedAuthor;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    public ShareRecipeViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;

        // Setup debounce timer for search (500ms delay)
        _searchDebounceTimer = new System.Timers.Timer(500);
        _searchDebounceTimer.Elapsed += async (s, e) =>
        {
            _searchDebounceTimer?.Stop();
            await LoadShareableAuthors();
        };
        _searchDebounceTimer.AutoReset = false;
    }

    public async Task Initialize()
    {
        await LoadShareableAuthors();
    }

    [RelayCommand]
    private async Task LoadShareableAuthors()
    {
        // Cancel any previous search
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _searchCancellationTokenSource.Token;

        IsSearching = true;
        try
        {
            var authors = await _recipeService.GetShareableAuthorsAsync(
                SearchTerm,
                8,
                cancellationToken);

            // Check if cancelled before updating UI
            if (!cancellationToken.IsCancellationRequested)
            {
                ShareableAuthors.Clear();
                foreach (var author in authors)
                {
                    ShareableAuthors.Add(author);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Search was cancelled, this is expected
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading shareable authors: {ex.Message}");
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IsSearching = false;
            }
        }
    }

    partial void OnSearchTermChanged(string value)
    {
        // Restart the debounce timer on each keystroke
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer?.Start();
    }

    [RelayCommand]
    private async Task ShareToUser(ShareableAuthorViewModel author)
    {
        SelectedAuthor = author;
        await PerformShare(author.AuthorId);
    }

    [RelayCommand]
    private async Task ShareUrl()
    {
        await PerformShare(null);
    }

    private async Task PerformShare(Guid? targetAuthorId)
    {
        if (!Guid.TryParse(RecipeId, out var recipeGuid))
        {
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _recipeService.ShareRecipeAsync(
                recipeGuid,
                targetAuthorId,
                CancellationToken.None);

            // Determine what to share
            string shareValue = targetAuthorId.HasValue
                ? response.ShareToken  // Share token for in-app sharing
                : response.ShareUrl;   // Original URL for external sharing

            // Use platform share functionality
            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default.RequestAsync(
                new Microsoft.Maui.ApplicationModel.DataTransfer.ShareTextRequest
                {
                    Text = shareValue,
                    Title = "Share Recipe"
                });

            // Close the share page after sharing
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error sharing recipe: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

