using System;

namespace MyCookbook.Common.ApiModels;

public readonly record struct AuthorModel(
    Guid Guid,
    string Name,
    string? Bio,
    string? Location,
    Uri? ProfileImageUri,
    Uri? BackgroundImageUri);

