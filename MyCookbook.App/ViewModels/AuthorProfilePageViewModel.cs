using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

[QueryProperty(nameof(AuthorGuid), nameof(AuthorGuid))]
[QueryProperty(nameof(PreviewName), nameof(PreviewName))]
[QueryProperty(nameof(PreviewImageUrl), nameof(PreviewImageUrl))]
[QueryProperty(nameof(PreviewBackgroundImageUrl), nameof(PreviewBackgroundImageUrl))]
public partial class AuthorProfilePageViewModel(
    ICookbookStorage cookbookStorage,
    CookbookHttpClient httpClient,
    ICognitoAuthService cognitoAuthService,
    IServiceProvider serviceProvider)
    : BaseViewModel
{
    private bool _hasPrePopulated = false;
    private bool _hasLoadedFullAuthor = false;

    [ObservableProperty]
    private AuthorModel? _author;

    [ObservableProperty]
    private AuthorViewModel? _authorViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetAuthorCommand))]
    private string? _authorGuid;

    [ObservableProperty]
    private string? _previewName;

    [ObservableProperty]
    private string? _previewImageUrl;

    [ObservableProperty]
    private string? _previewBackgroundImageUrl;

    [ObservableProperty]
    private Guid _currentUserAuthorGuid;

    [ObservableProperty]
    private bool _isCurrentUser;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    public string RecipesTabTitle => IsCurrentUser ? "My Recipes" : "Recipes";

    partial void OnAuthorChanged(AuthorModel? value)
    {
        if (value.HasValue)
        {
            var author = value.Value;
            AuthorViewModel = new AuthorViewModel(author);
            _hasLoadedFullAuthor = true;
        }
    }

    partial void OnAuthorGuidChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out var guid))
        {
            CheckIfCurrentUser(guid);
        }
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(RecipesTabTitle));
    }

    private async void CheckIfCurrentUser(Guid authorGuid)
    {
        var currentUser = await cookbookStorage.GetUser();
        if (currentUser.HasValue)
        {
            // For now, we'll use the user's Guid as the author Guid
            // In a real scenario, you'd have a mapping from User to Author
            CurrentUserAuthorGuid = currentUser.Value.Guid;
            IsCurrentUser = authorGuid == currentUser.Value.Guid;
            OnPropertyChanged(nameof(RecipesTabTitle));
        }
    }

    public void PrePopulateFromPreviewData()
    {
        System.Diagnostics.Debug.WriteLine($"PrePopulateFromPreviewData: _hasPrePopulated={_hasPrePopulated}, _hasLoadedFullAuthor={_hasLoadedFullAuthor}");

        if (_hasPrePopulated || _hasLoadedFullAuthor)
        {
            System.Diagnostics.Debug.WriteLine("PrePopulateFromPreviewData: Skipping - already populated or loaded");
            return;
        }

        // Pre-populate with any available preview data
        if (!string.IsNullOrEmpty(PreviewName) || !string.IsNullOrEmpty(PreviewImageUrl))
        {
            System.Diagnostics.Debug.WriteLine($"PrePopulateFromPreviewData: Has preview data - Name: {PreviewName}, ImageUrl: {PreviewImageUrl}");

            Uri? profileImageUri = null;
            if (!string.IsNullOrEmpty(PreviewImageUrl) && Uri.TryCreate(PreviewImageUrl, UriKind.Absolute, out var tempUri))
            {
                profileImageUri = tempUri;
            }

            Uri? backgroundImageUri = null;
            if (!string.IsNullOrEmpty(PreviewBackgroundImageUrl) && Uri.TryCreate(PreviewBackgroundImageUrl, UriKind.Absolute, out var tempBgUri))
            {
                backgroundImageUri = tempBgUri;
            }

            // Create a minimal AuthorModel for preview
            var previewAuthor = new AuthorModel(
                Guid: Guid.Empty,
                Name: PreviewName ?? string.Empty,
                Bio: null,
                Location: null,
                ProfileImageUri: profileImageUri,
                BackgroundImageUri: backgroundImageUri);

            // Set both Author and AuthorViewModel for preview display
            Author = previewAuthor;
            AuthorViewModel = new AuthorViewModel(previewAuthor);
            System.Diagnostics.Debug.WriteLine($"PrePopulateFromPreviewData: Set preview author: {Author?.Name}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("PrePopulateFromPreviewData: No preview data available");
        }

        _hasPrePopulated = true;
        System.Diagnostics.Debug.WriteLine($"PrePopulateFromPreviewData: Triggering GetAuthorCommand (AuthorGuid={AuthorGuid})");

        // Check if command can execute
        if (GetAuthorCommand.CanExecute(null))
        {
            System.Diagnostics.Debug.WriteLine("PrePopulateFromPreviewData: GetAuthorCommand.CanExecute = true, executing...");
            _ = GetAuthorCommand.ExecuteAsync(null);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("PrePopulateFromPreviewData: GetAuthorCommand.CanExecute = false, NOT executing");
        }
    }

    [RelayCommand]
    private async Task GetAuthor()
    {
        if (string.IsNullOrEmpty(AuthorGuid))
        {
            System.Diagnostics.Debug.WriteLine("GetAuthor: AuthorGuid is null/empty, returning");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"GetAuthor: Starting API call for AuthorGuid: {AuthorGuid}");
        IsBusy = true;
        try
        {
            Author = await httpClient.Get<AuthorModel>(
                new Uri(
                    $"/api/Author/{AuthorGuid}",
                    UriKind.Absolute),
                CancellationToken.None);
            _hasLoadedFullAuthor = true;
            System.Diagnostics.Debug.WriteLine($"GetAuthor: Successfully loaded author: {Author?.Name}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAuthor: Error loading author: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GetAuthor: Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectRecipesTab()
    {
        SelectedTabIndex = 0;
    }

    [RelayCommand]
    private void SelectCookbookTab()
    {
        SelectedTabIndex = 1;
    }

    [RelayCommand]
    private async Task Settings()
    {
        var currentPage = Application.Current?.MainPage;
        if (currentPage == null)
        {
            return;
        }

        var popup = new Views.Profile.Settings(cookbookStorage, cognitoAuthService);
        await currentPage.ShowPopupAsync(popup);
    }

    /// <summary>
    /// Reset the ViewModel state for fresh loading.
    /// Call this when navigating away from the page.
    /// </summary>
    public void ResetState()
    {
        System.Diagnostics.Debug.WriteLine("ResetState: Clearing all ViewModel state");
        _hasPrePopulated = false;
        _hasLoadedFullAuthor = false;
        AuthorGuid = null;
        PreviewName = null;
        PreviewImageUrl = null;
        PreviewBackgroundImageUrl = null;
        Author = null;
        AuthorViewModel = null;
        System.Diagnostics.Debug.WriteLine("ResetState: State cleared");
    }
}

