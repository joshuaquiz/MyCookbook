using Microsoft.Maui.Controls;
using MyCookbook.App.Components.RecipeSummary;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
//using Plugin.MauiMTAdmob.Extra;

namespace MyCookbook.App.Views.Home;

public partial class HomePage
{
    private readonly int _pageSize;
    private readonly ICookbookStorage _cookbookStorage;
    private readonly CookbookHttpClient _httpClient;
    private readonly System.Timers.Timer? _searchTimer;

    private bool _isSearchBarVisible = true;
    private DateTimeOffset _lastScrollFilterVisibilityChangeTimeout = DateTimeOffset.UtcNow;

    public HomePage(
        ICookbookStorage cookbookStorage,
        CookbookHttpClient httpClient)
    {
        _pageSize = 20;
        _cookbookStorage = cookbookStorage;
        _httpClient = httpClient;
        InitializeComponent();
        GetData = RecipeSummaryListComponent_OnGetData;
        _searchTimer = new System.Timers.Timer(500); // 0.5 seconds
        _searchTimer.Elapsed += OnSearchTimerElapsed;
        _searchTimer.AutoReset = false;
        RecipeList.Loaded += (_, _) =>
        {
            if (RecipeList.Content is Grid grid
                && grid.Children[0] is RefreshView { Content: CollectionView collectionView })
            {
                collectionView.Scrolled += OnCollectionViewScrolled;
            }
        };
    }

    private async void OnCollectionViewScrolled(object? sender, ItemsViewScrolledEventArgs e)
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
        FilterSection.IsVisible = false;
        SearchGrid.HeightRequest = 0;
        SearchGrid.TranslateToAsync(0, -SearchGrid.Height - 20, 1000, Easing.CubicOut);
    }

    private void ShowSearchBar()
    {
        _isSearchBarVisible = true;
        SearchGrid.HeightRequest = -1; // Reset to Auto
        SearchGrid.TranslateToAsync(0, 0, 1000, Easing.CubicOut);
    }

    private void OnSearchTimerElapsed(object? sender, ElapsedEventArgs e) =>
        MainThread.InvokeOnMainThreadAsync(() =>
            RecipeList.RefreshData(CancellationToken.None));

    private async IAsyncEnumerable<RecipeSummaryViewModel> RecipeSummaryListComponent_OnGetData(
        int pageNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ValueTask<List<RecipeSummaryViewModel>> data;
        if (!string.IsNullOrEmpty(TextSearchEntry.Text))
        {
            data = _httpClient.Get<List<RecipeSummaryViewModel>>(
                new Uri(
                    $"/api/Search/Global?term={TextSearchEntry.Text}&category=&ingredient={IncludeIngredientsEntry.Text}&exclude={ExcludeIngredientsEntry.Text}&take={_pageSize}&skip={pageNumber * _pageSize}",
                    UriKind.Absolute),
                cancellationToken);
        }
        else
        {
            data = _httpClient.Get<List<RecipeSummaryViewModel>>(
                new Uri(
                    $"/api/Home/Popular?take={_pageSize}&skip={_pageSize * pageNumber}",
                    UriKind.Relative),
                cancellationToken);
        }

        foreach (var item in await data)
        {
            yield return item;
        }
    }

    private Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>? _getData;

    public Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>? GetData
    {
        get => _getData;
        set
        {
            _getData = value;
            OnPropertyChanged();
        }
    }

    private void TextSearchEntry_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    private void ClearButton_OnClicked(object? sender, EventArgs e)
    {
        var oldValue = TextSearchEntry.Text;
        TextSearchEntry.Text = string.Empty;
        IncludeIngredientsEntry.Text = string.Empty;
        ExcludeIngredientsEntry.Text = string.Empty;
        // Reset categories selection
        // Trigger refresh
        _searchTimer?.Stop();
        _searchTimer?.Start();
        TextSearchEntry_OnTextChanged(TextSearchEntry, new TextChangedEventArgs(oldValue, string.Empty));
    }

    private void FilterButton_OnClicked(object? sender, EventArgs e)
    {
        FilterSection.IsVisible = !FilterSection.IsVisible;
    }

    private void CategoryButton_OnClicked(object? sender, EventArgs e)
    {
        // Handle category selection
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    private void IncludeIngredientsEntry_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }

    private void ExcludeIngredientsEntry_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchTimer?.Stop();
        _searchTimer?.Start();
    }
}
