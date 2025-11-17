using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System;
using MyCookbook.App.Components.RecipeSummary;
using MyCookbook.App.Implementations;
using System.Windows.Input;
using System.Collections.ObjectModel;
using MyCookbook.App.Implementations.Models;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace MyCookbook.App.Views.Search;

public partial class SearchHome
{
    private readonly int _pageSize;
    private readonly CookbookHttpClient _httpClient;

    private string? _term;
    private string? _category;
    private string? _ingredient;
    private CancellationTokenSource _cts;

    public SearchHome(
        CookbookHttpClient httpClient)
    {
        _cts = new CancellationTokenSource();
        _pageSize = 20;
        _httpClient = httpClient;
        InitializeComponent();
        Categories = [];
        Ingredients = [];
        GetData = RecipeSummaryListComponent_OnGetData;
    }

    private async Task ResetCancellationTokenSource()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
    }

    private void OnLoaded(
        object? sender,
        EventArgs e)
    {
        GetCategories();
        GetIngredientsCommand.Execute(null);
    }

    public ICommand SearchCommand =>
        new AsyncRelayCommand<string>(
            async term =>
            {
                await ResetCancellationTokenSource();
                _term = term;
                /*await RecipeSummaryListControl?.RefreshData(
                    _cts.Token)!;*/
            });

    private async IAsyncEnumerable<RecipeSummaryViewModel> RecipeSummaryListComponent_OnGetData(
        int pageNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var data = _httpClient.Get<List<RecipeSummaryViewModel>>(
            new Uri(
                $"/api/Search/Global?term={_term}&category={_category}&ingredient={_ingredient}&take={_pageSize}&skip={pageNumber * _pageSize}",
                UriKind.Absolute),
            cancellationToken);
        foreach (var item in await data)
        {
            yield return item;
        }
    }

    private void GetCategories()
    {
        Categories
            ?.Add(
                new SearchCategoryItem
                {
                    Name = "Drinks",
                    ColorHex = "#D36E70"
                });
        Categories
            ?.Add(
                new SearchCategoryItem
                {
                    Name = "Dinners",
                    ColorHex = "#2A6478"
                });
        Categories
            ?.Add(
                new SearchCategoryItem
                {
                    Name = "Breakfasts",
                    ColorHex = "#3F888F"
                });
        Categories
            ?.Add(
                new SearchCategoryItem
                {
                    Name = "Lunches",
                    ColorHex = "#317F43"
                });
        Categories
            ?.Add(
                new SearchCategoryItem
                {
                    Name = "Snacks",
                    ColorHex = "#D84B20"
                });
    }

    private ICommand GetIngredientsCommand =>
        new AsyncRelayCommand(
            GetIngredients);

    private async Task GetIngredients(
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        var data = await _httpClient.Get<List<SearchCategoryItem>>(
            new Uri(
                "/api/Search/Ingredients",
                UriKind.Absolute),
            cancellationToken);
        foreach (var item in data)
        {
            Ingredients?.Add(item);
        }

        IsBusy = false;
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

    private ObservableCollection<SearchCategoryItem>? _categories;

    public ObservableCollection<SearchCategoryItem>? Categories
    {
        get => _categories;
        set
        {
            _categories = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<SearchCategoryItem>? _ingredients;

    public ObservableCollection<SearchCategoryItem>? Ingredients
    {
        get => _ingredients;
        set
        {
            _ingredients = value;
            OnPropertyChanged();
        }
    }

    public ICommand CategorySelectedCommand =>
        new AsyncRelayCommand<string>(
            CategorySelected);

    private async Task CategorySelected(
        string? categoryName)
    {
        await ResetCancellationTokenSource();
        _category = _category == categoryName
            ? null
            : categoryName;
        /*await RecipeSummaryListControl
            ?.RefreshData(
                _cts.Token)!;*/
    }

    public ICommand IngredientSelectedCommand =>
        new AsyncRelayCommand<string>(
            IngredientSelected);

    private async Task IngredientSelected(
        string? ingredientName)
    {
        await ResetCancellationTokenSource();
        _ingredient = _ingredient == ingredientName
            ? null
            : ingredientName;
        await RecipeSummaryListControl
            ?.RefreshData(
                _cts.Token)!;
    }
}