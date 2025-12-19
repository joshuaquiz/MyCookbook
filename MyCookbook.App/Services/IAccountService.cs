using System.Threading;
using System.Threading.Tasks;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Service for account-related API operations
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Log in with username and password
    /// </summary>
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}

