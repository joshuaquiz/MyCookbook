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

[QueryProperty(nameof(RecipeId), nameof(RecipeId))]
[QueryProperty(nameof(RecipeUrl), nameof(RecipeUrl))]
public partial class ShareRecipeViewModel : BaseViewModel
{
    private readonly CookbookHttpClient _httpClient;

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

    public ShareRecipeViewModel(CookbookHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Initialize()
    {
        await LoadShareableAuthors();
    }

    [RelayCommand]
    private async Task LoadShareableAuthors()
    {
        IsSearching = true;
        try
        {
            var authors = await _httpClient.Get<List<ShareableAuthorViewModel>>(
                new Uri(
                    $"/api/Recipe/ShareableAuthors?searchTerm={Uri.EscapeDataString(SearchTerm)}&take=8",
                    UriKind.Relative),
                CancellationToken.None);

            ShareableAuthors.Clear();
            foreach (var author in authors)
            {
                ShareableAuthors.Add(author);
            }
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading shareable authors: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    partial void OnSearchTermChanged(string value)
    {
        // Debounce search
        _ = LoadShareableAuthors();
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
            var request = new ShareRecipeRequest(SharedToAuthorId: targetAuthorId);
            var response = await _httpClient.Post<ShareRecipeRequest, ShareRecipeResponse>(
                new Uri($"/api/Recipe/{recipeGuid}/Share", UriKind.Relative),
                request,
                CancellationToken.None);

            // Determine what to share
            string shareValue = targetAuthorId.HasValue
                ? response.ShareToken  // Share token for in-app sharing
                : response.ShareUrl;   // Original URL for external sharing

            // Use platform share functionality
            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default.RequestAsync(
                new ShareTextRequest
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

