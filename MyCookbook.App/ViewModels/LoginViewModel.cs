using System;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ICookbookStorage _cookbookStorage;
    private readonly CookbookHttpClient _httpClient;

    public LoginViewModel(
        ICookbookStorage cookbookStorage,
        CookbookHttpClient httpClient)
    {
        _cookbookStorage = cookbookStorage;
        _httpClient = httpClient;
    }

    public async Task LogIn(
        string username,
        string password)
    {
        IsBusy = true;
        try
        {
            var user = await _httpClient.Post<UserProfileModel, object>(
                new Uri(
                    "/api/Account/LogIn",
                    UriKind.Absolute),
                new
                {
                    Username = username,
                    Password = password
                },
                new CancellationTokenSource(
                        TimeSpan.FromMinutes(
                            1))
                    .Token);
            await _cookbookStorage.SetUser(user);
        }
        finally
        {
            IsBusy = false;
        }
    }
}