using System;
using System.Collections.Generic;

namespace MyCookbook.Common;

public sealed record UserProfile(
    Guid Guid,
    Uri BackgroundImageUri,
    Uri ProfileImageUri,
    string FirstName,
    string LastName,
    string Country,
    string City,
    int Age,
    int RecipesAdded,
    string? Description,
    bool IsPremium,
    bool IsFollowed,
    List<PopularItem> RecentRecipes);