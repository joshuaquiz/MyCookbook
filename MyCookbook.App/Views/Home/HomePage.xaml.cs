using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Maui.Controls;
using MyCookbook.App.Components.RecipeSummary;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Views.Profile;
using MyCookbook.Common;
//using Plugin.MauiMTAdmob.Extra;

namespace MyCookbook.App.Views.Home;

public partial class HomePage
{
    private readonly int _pageSize;
    private readonly ICookbookStorage _cookbookStorage;
    private readonly CookbookHttpClient _httpClient;

    public HomePage(
        ICookbookStorage cookbookStorage,
        CookbookHttpClient httpClient)
    {
        _pageSize = 20;
        _cookbookStorage = cookbookStorage;
        _httpClient = httpClient;
        InitializeComponent();
        GetData = RecipeSummaryListComponent_OnGetData;
        //MyAdView.LoadAd(CollapsibleBannerMode.Bottom);
    }

    private async void HomeBar_OnNavigate(string arg)
    {
        if (arg == nameof(ProfileHome))
        {
            var userProfile = await _cookbookStorage.GetUser();
            await Shell.Current.GoToAsync(
                arg,
                new Dictionary<string, object>
                {
                    {
                        nameof(UserProfile),
                        userProfile!
                    }
                });
        }
        else
        {
            await Shell.Current.GoToAsync(arg);
        }
    }

    private async IAsyncEnumerable<RecipeSummaryViewModel> RecipeSummaryListComponent_OnGetData(
        int pageNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var data = _httpClient.Get<List<RecipeSummaryViewModel>>(
            new Uri(
                $"/api/Home/Popular?take={_pageSize}&skip={_pageSize * pageNumber}",
                UriKind.Absolute),
            cancellationToken);
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
}