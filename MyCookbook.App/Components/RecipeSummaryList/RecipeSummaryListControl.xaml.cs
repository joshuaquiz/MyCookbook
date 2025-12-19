using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private bool _endOfList;
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

    public static readonly BindableProperty IsIdleProperty =
        BindableProperty.Create(nameof(IsIdle), typeof(bool), typeof(RecipeSummaryListControl));

    public bool IsIdle
    {
        get => (bool)GetValue(IsIdleProperty);
        set => SetValue(IsIdleProperty, value);
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
        Debug.WriteLine("[RecipeSummaryListControl] RefreshData called");
        IsRefreshing = true;
        _endOfList = false;
        Items.Clear();
        Count = 0;
        _pageNumber = 0;
        Debug.WriteLine("[RecipeSummaryListControl] Calling GetDataPage");
        await GetDataPage(
            cancellationToken);
        IsRefreshing = false;
        Debug.WriteLine("[RecipeSummaryListControl] RefreshData completed");
    }

    public ICommand GetNextPageCommand =>
        new AsyncRelayCommand(
            GetDataPage);

    private async Task GetDataPage(
        CancellationToken cancellationToken)
    {
        Debug.WriteLine($"[RecipeSummaryListControl] GetDataPage called - IsBusy: {IsBusy}, EndOfList: {_endOfList}, GetData is null: {GetData == null}");

        if (IsBusy || _endOfList || GetData == null)
        {
            Debug.WriteLine("[RecipeSummaryListControl] GetDataPage exiting early");
            return;
        }

        IsIdle = false;
        IsBusy = true;
        var scrollToTop = !Items.Any();
        const int maxItems = 250;
        var itemsInPage = 0;

        Debug.WriteLine($"[RecipeSummaryListControl] Starting to fetch page {_pageNumber}");

        try
        {
            await foreach (var item in GetData(_pageNumber, cancellationToken).WithCancellation(cancellationToken))
            {
                Items.Add(item);
                Count++;
                itemsInPage++;
            }

            Debug.WriteLine($"[RecipeSummaryListControl] Fetched {itemsInPage} items");

            while (Items.Count > maxItems)
            {
                Items.RemoveAt(0);
                Count--;
            }
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("[RecipeSummaryListControl] Task was cancelled");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RecipeSummaryListControl] Error fetching data: {ex.Message}");
        }

        if (scrollToTop)
        {
            Cv.ScrollTo(0);
        }

        if (itemsInPage < 5)
        {
            _endOfList = true;
        }

        _pageNumber++;
        IsBusy = false;
        PagingTimeout = DateTimeOffset.UtcNow.AddMilliseconds(5);
        IsIdle = true;

        Debug.WriteLine($"[RecipeSummaryListControl] GetDataPage completed - Total items: {Items.Count}");
    }

    public DateTimeOffset PagingTimeout { get; set; }
}