namespace MyCookbook.Common.ApiModels;

public record LoginResponse(
    UserProfileModel UserProfile,
    string AccessToken,
    string RefreshToken,
    string TokenType = "Bearer",
    int ExpiresIn = 7200 // 2 hours in seconds
);

