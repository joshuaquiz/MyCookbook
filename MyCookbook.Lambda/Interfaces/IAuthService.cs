using MyCookbook.Lambda.Models;

namespace MyCookbook.Lambda.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}

