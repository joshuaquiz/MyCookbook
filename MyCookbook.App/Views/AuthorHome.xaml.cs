using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyCookbook.App.Components.RecipeSummary;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class AuthorHome
{
    private readonly int _pageSize;

    private readonly ICookbookStorage _cookbookStorage;
    private readonly CookbookHttpClient _httpClient;

    private AuthorProfilePageViewModel ViewModel { get; }

    private bool _hasLoadedProfile;

    public AuthorHome(
        ICookbookStorage cookbookStorage,
        CookbookHttpClient httpClient,
        AuthorProfilePageViewModel viewModel)
    {
        _pageSize = 20;
        _cookbookStorage = cookbookStorage;
        _httpClient = httpClient;
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
        GetRecipesData = RecipesListComponent_OnGetData;
        GetCookbookData = CookbookListComponent_OnGetData;
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"LoadProfileAsync - AuthorGuid: {ViewModel.AuthorGuid}");

            // If AuthorGuid is not set, use the current user's GUID
            if (string.IsNullOrEmpty(ViewModel.AuthorGuid))
            {
                System.Diagnostics.Debug.WriteLine("AuthorGuid is null/empty, loading current user");
                var currentUser = await _cookbookStorage.GetUser();
                if (currentUser.HasValue)
                {
                    // Set the AuthorGuid to the current user's GUID
                    // This will trigger the CheckIfCurrentUser method in the ViewModel
                    ViewModel.AuthorGuid = currentUser.Value.Guid.ToString();
                    System.Diagnostics.Debug.WriteLine($"Set AuthorGuid to current user: {ViewModel.AuthorGuid}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No current user found");
                    return;
                }
            }

            // Pre-populate with preview data if available (this sets the UI immediately)
            ViewModel.PrePopulateFromPreviewData();

            // Note: PrePopulateFromPreviewData already triggers GetAuthorCommand internally
            // We don't need to call it again here
        }
        catch (Exception)
        {
            // Silently handle errors during profile loading
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Load profile data on first appearance or when AuthorGuid changes
            // This handles both TabBar navigation (no AuthorGuid) and detail navigation (with AuthorGuid)
            if (!_hasLoadedProfile)
            {
                _hasLoadedProfile = true;
                await LoadProfileAsync();
            }

            // Configure back button visibility based on navigation context
            ConfigureBackButton();

            // Check if this is the current user's profile to show premium option
            var currentUser = await _cookbookStorage.GetUser();
            if (currentUser.HasValue &&
                ViewModel.AuthorGuid == currentUser.Value.Guid.ToString())
            {
                // For now, we'll check the user's premium status
                // In a real scenario, you'd check the author's premium status
                if (!currentUser.Value.IsPremium)
                {
                    // Check if premium button already exists
                    var hasPremiumButton = false;
                    foreach (var item in ToolbarItems)
                    {
                        if (item.Text == "Premium")
                        {
                            hasPremiumButton = true;
                            break;
                        }
                    }

                    if (!hasPremiumButton)
                    {
                        ToolbarItems.Add(
                            new ToolbarItem(
                                "Premium",
                                "chef_hat",
                                Premium_Clicked));
                    }
                }
            }

            // Add "Back to Home" button if navigation stack is deep (> 2 levels)
            AddBackToHomeButtonIfNeeded();
        }
        catch (ObjectDisposedException)
        {
            // Page is being disposed, ignore navigation configuration
        }
        catch (Exception)
        {
            // Silently handle errors in OnAppearing
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // When navigating away from the page via back button or tab switch,
        // reset the flag so the profile reloads on next appearance
        // Note: We DON'T reset ViewModel state here because:
        // 1. For Transient pages, a new ViewModel instance is created each time
        // 2. For TabBar navigation, we want to reload fresh data anyway
        _hasLoadedProfile = false;
    }

    private void ConfigureBackButton()
    {
        try
        {
            // Get the navigation stack - use safe navigation
            var shell = Shell.Current;
            if (shell == null) return;

            var navigation = shell.Navigation;
            if (navigation == null) return;

            var navigationStack = navigation.NavigationStack;
            if (navigationStack == null) return;

            // If we're at the root of the navigation stack (accessed via TabBar),
            // hide the back button and show the TabBar.
            // Otherwise, show the back button and hide the TabBar (detail page navigation).
            if (navigationStack.Count <= 1)
            {
                // We're at the root (TabBar navigation)
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior
                {
                    IsVisible = false
                });
                Shell.SetTabBarIsVisible(this, true);
            }
            else
            {
                // We're in the navigation stack (navigated from a recipe as detail page)
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior
                {
                    IsVisible = true
                });
                Shell.SetTabBarIsVisible(this, false);
            }
        }
        catch (ObjectDisposedException)
        {
            // Shell or navigation is disposed, ignore
        }
    }

    private void AddBackToHomeButtonIfNeeded()
    {
        try
        {
            var shell = Shell.Current;
            if (shell == null) return;

            var navigation = shell.Navigation;
            if (navigation == null) return;

            var navigationStack = navigation.NavigationStack;
            if (navigationStack == null) return;

            // If navigation stack depth > 2, add a "Back to Home" button as an escape hatch
            if (navigationStack.Count > 2)
            {
                // Check if we already have this button
                var hasHomeButton = false;
                foreach (var item in ToolbarItems)
                {
                    if (item.Text == "Home")
                    {
                        hasHomeButton = true;
                        break;
                    }
                }

                if (!hasHomeButton)
                {
                    ToolbarItems.Insert(0, new ToolbarItem(
                        "Home",
                        "home.png",
                        async void () =>
                        {
                            try
                            {
                                await Shell.Current.GoToAsync("//HomePage");
                            }
                            catch (Exception)
                            {
                                // Silently handle navigation errors
                            }
                        }));
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Shell or navigation is disposed, ignore
        }
    }

    private void Premium_Clicked()
    {
        throw new NotImplementedException();
    }

    private Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>? _getRecipesData;

    public Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>? GetRecipesData
    {
        get => _getRecipesData;
        set
        {
            _getRecipesData = value;
            OnPropertyChanged();
        }
    }

    private Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>? _getCookbookData;

    public Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>? GetCookbookData
    {
        get => _getCookbookData;
        set
        {
            _getCookbookData = value;
            OnPropertyChanged();
        }
    }

    private async IAsyncEnumerable<RecipeSummaryViewModel> RecipesListComponent_OnGetData(
        int pageNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var data = _httpClient.Get<List<RecipeSummaryViewModel>>(
            new Uri(
                $"/api/Author/{ViewModel.AuthorGuid}/Recipes?take={_pageSize}&skip={pageNumber * _pageSize}",
                UriKind.Absolute),
            cancellationToken);
        foreach (var item in await data)
        {
            yield return item;
        }
    }

    private async IAsyncEnumerable<RecipeSummaryViewModel> CookbookListComponent_OnGetData(
        int pageNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var data = _httpClient.Get<List<RecipeSummaryViewModel>>(
            new Uri(
                $"/api/Author/{ViewModel.AuthorGuid}/Cookbook?take={_pageSize}&skip={pageNumber * _pageSize}",
                UriKind.Absolute),
            cancellationToken);
        foreach (var item in await data)
        {
            yield return item;
        }
    }
}