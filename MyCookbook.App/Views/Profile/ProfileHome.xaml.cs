using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using MyCookbook.App.Components.RecipeSummary;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views.Profile;

public partial class ProfileHome
{
    private readonly int _pageSize;

    private readonly ICookbookStorage _cookbookStorage;
    private readonly CookbookHttpClient _httpClient;

    private ProfileViewModel ViewModel { get; }

    public ProfileHome(
        ICookbookStorage cookbookStorage,
        CookbookHttpClient httpClient,
        ProfileViewModel viewModel)
    {
        _pageSize = 20;
        _cookbookStorage = cookbookStorage;
        _httpClient = httpClient;
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
        GetData = RecipeSummaryListComponent_OnGetData;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (ViewModel.UserProfile?.Guid == (await _cookbookStorage.GetUser())!.Guid)
        {
            ToolbarItems.Add(
                new ToolbarItem(
                    "Settings",
                    "gear",
                    Settings_Clicked));
            ToolbarItems.Add(
                new ToolbarItem(
                    "Log out",
                    "logout",
                    Logout_Clicked));
            if (!ViewModel.UserProfile.IsPremium)
            {
                ToolbarItems.Add(
                    new ToolbarItem(
                        "Premium",
                        "chef_hat",
                        Premium_Clicked));
            }
        }
    }

    private async void Settings_Clicked() =>
        await this.ShowPopupAsync(
            new Settings(
                _cookbookStorage));

    private void Premium_Clicked()
    {
        throw new NotImplementedException();
    }

    private async void Logout_Clicked()
    {
        await ViewModel.Logout();
        Application.Current!.MainPage = new Login(
            new LoginViewModel(
                _cookbookStorage,
                _httpClient));
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

    private async IAsyncEnumerable<RecipeSummaryViewModel> RecipeSummaryListComponent_OnGetData(
        int pageNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var data = _httpClient.Get<List<RecipeSummaryViewModel>>(
            new Uri(
                $"/api/Account/{ViewModel.Guid}/Cookbook/?take={_pageSize}&skip={pageNumber * _pageSize}",
                UriKind.Absolute),
            cancellationToken);
        foreach (var item in await data)
        {
            yield return item;
        }
    }
}