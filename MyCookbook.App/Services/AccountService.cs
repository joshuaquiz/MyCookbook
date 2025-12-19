using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Implementations;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Implementation of account-related API operations
/// </summary>
public class AccountService : IAccountService
{
    private readonly CookbookHttpClient _httpClient;

    public AccountService(CookbookHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.Post<LoginResponse, object>(
            "/api/Account/LogIn",
            new
            {
                Username = username,
                Password = password
            },
            cancellationToken);
    }
}

