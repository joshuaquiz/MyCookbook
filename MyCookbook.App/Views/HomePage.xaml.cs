using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class HomePage
{
    private bool _isSearchBarVisible = true;
    private DateTimeOffset _lastScrollFilterVisibilityChangeTimeout = DateTimeOffset.UtcNow;
    private bool _hasInitialized;

    public HomePageViewModel ViewModel { get; }

    public HomePage(HomePageViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();

        RecipeList.GetData = ViewModel.GetRecipeData;

        ViewModel.TriggerRefresh = () =>
            MainThread.InvokeOnMainThreadAsync(() =>
                RecipeList.RefreshData(CancellationToken.None));

        RecipeList.Loaded += (_, _) =>
        {
            if (RecipeList.Content is Grid grid
                && grid.Children[0] is RefreshView { Content: CollectionView collectionView })
            {
                collectionView.Scrolled += OnCollectionViewScrolled;
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Load data asynchronously on first appearance
        if (!_hasInitialized)
        {
            _hasInitialized = true;
            _ = RecipeList.RefreshData(CancellationToken.None);
        }
    }

    private void OnCollectionViewScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        var now = DateTimeOffset.UtcNow;
        if (e.VerticalOffset == 0
            || Math.Abs(e.VerticalDelta) < 10
            || RecipeList.IsBusy
            || RecipeList.PagingTimeout > now
            || _lastScrollFilterVisibilityChangeTimeout > now)
        {
            return;
        }

        if (e.VerticalDelta > 0 && _isSearchBarVisible)
        {
            HideSearchBar();
        }
        else if (e.VerticalDelta < 0 && !_isSearchBarVisible)
        {
            ShowSearchBar();
        }

        _lastScrollFilterVisibilityChangeTimeout = DateTimeOffset.UtcNow.AddMilliseconds(500);
    }

    private void HideSearchBar()
    {
        _isSearchBarVisible = false;
        ViewModel.IsFilterVisible = false;
        SearchGrid.HeightRequest = 0;
        SearchGrid.TranslateToAsync(0, -SearchGrid.Height - 20, 1000, Easing.CubicOut);
    }

    private void ShowSearchBar()
    {
        _isSearchBarVisible = true;
        SearchGrid.HeightRequest = -1; // Reset to Auto
        SearchGrid.TranslateToAsync(0, 0, 1000, Easing.CubicOut);
    }
}
