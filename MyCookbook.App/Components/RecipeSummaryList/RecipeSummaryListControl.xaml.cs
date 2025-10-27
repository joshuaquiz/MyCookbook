using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MyCookbook.App.Components.RecipeSummary;

namespace MyCookbook.App.Components.RecipeSummaryList;

public partial class RecipeSummaryListControl
{
    private int _pageNumber;

    public static readonly BindableProperty CountProperty =
        BindableProperty.Create(nameof(Count), typeof(int), typeof(RecipeSummaryListControl));

    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(RecipeSummaryListControl));

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly BindableProperty IsRefreshingProperty =
        BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(RecipeSummaryListControl));

    public bool IsRefreshing
    {
        get => (bool)GetValue(IsRefreshingProperty);
        set => SetValue(IsRefreshingProperty, value);
    }

    public static readonly BindableProperty ItemsProperty =
        BindableProperty.Create(nameof(Items), typeof(ObservableCollection<RecipeSummaryViewModel>), typeof(RecipeSummaryListControl));

    public ObservableCollection<RecipeSummaryViewModel> Items
    {
        get => (ObservableCollection<RecipeSummaryViewModel>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly BindableProperty GetDataProperty =
        BindableProperty.Create(
            nameof(GetData),
            typeof(Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>),
            typeof(RecipeSummaryListControl));

    public Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>> GetData
    {
        get => (Func<int, CancellationToken, IAsyncEnumerable<RecipeSummaryViewModel>>)GetValue(GetDataProperty);
        set => SetValue(GetDataProperty, value);
    }

    public RecipeSummaryListControl()
    {
        InitializeComponent();
        Items = [];
        BindingContext = this;
    }

    public ICommand RefreshCommand =>
        new AsyncRelayCommand(
            RefreshData);

    public async Task RefreshData(
        CancellationToken cancellationToken)
    {
        IsRefreshing = true;
        Items.Clear();
        Count = 0;
        _pageNumber = 0;
        await GetDataPage(
            cancellationToken);
        IsRefreshing = false;
    }

    public ICommand GetNextPageCommand =>
        new AsyncRelayCommand(
            GetDataPage);

    private async Task GetDataPage(
        CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        var scrollToTop = !Items.Any();
        const int maxItems = 250;
        try
        {
            await foreach (var item in GetData(_pageNumber, cancellationToken).WithCancellation(cancellationToken))
            {
                Items.Add(item);
                Count++;
            }

            while (Items.Count > maxItems)
            {
                Items.RemoveAt(0);
                Count--;
            }
        }
        catch (TaskCanceledException)
        {
            // Do nothing.
        }

        if (scrollToTop)
        {
            Cv.ScrollTo(0);
        }

        _pageNumber++;
        IsBusy = false;
    }
}