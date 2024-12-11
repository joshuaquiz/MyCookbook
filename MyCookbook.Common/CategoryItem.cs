using System;

namespace MyCookbook.Common;

public sealed record CategoryItem(
    Uri? ImageUrl,
    string ColorHex,
    string Name);