namespace MyCookbook.Common.ApiModels;

public record LoginResponse(
    UserProfileModel UserProfile,
    string AccessToken,
    string RefreshToken,
    string TokenType = "Bearer",
    int ExpiresIn = 3600 // 1 hour in seconds
);

