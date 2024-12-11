using System;
using MyCookbook.App.Views.MyCookbook;
using MyCookbook.App.Views.Profile;
using MyCookbook.App.Views.Search;

namespace MyCookbook.App.Views.Home;

public partial class HomeBar
{
    public event Action<string>? Navigate;

    public HomeBar()
    {
        InitializeComponent();
    }

    private void Search_OnClicked(object? sender, EventArgs e) =>
        Navigate?.Invoke(nameof(SearchHome));

    private void Shopping_OnClicked(object? sender, EventArgs e) =>
        Navigate?.Invoke(nameof(ShoppingListHome));

    private void Cookbook_OnClicked(object? sender, EventArgs e) =>
        Navigate?.Invoke(nameof(MyCookbookHome));

    private void Calendar_OnClicked(object? sender, EventArgs e) =>
        Navigate?.Invoke(nameof(CalendarHome));

    private void Profile_OnClicked(object? sender, EventArgs e) =>
        Navigate?.Invoke(nameof(ProfileHome));
}