using System;
using System.Collections.Generic;

namespace MyCookbook.Common.ApiModels;

public readonly record struct UserProfileModel(
    Guid Guid,
    Uri? BackgroundImageUri,
    Uri? ProfileImageUri,
    string FirstName,
    string LastName,
    string Country,
    string City,
    int Age,
    int RecipesAdded,
    string? Description,
    bool IsPremium,
    bool IsFollowed,
    IReadOnlyList<PopularItem> RecentRecipes);