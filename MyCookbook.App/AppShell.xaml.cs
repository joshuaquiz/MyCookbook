using Microsoft.Maui.Controls;
using MyCookbook.App.Views;
using MyCookbook.App.Views.Home;
using MyCookbook.App.Views.MyCookbook;
using MyCookbook.App.Views.Profile;
using MyCookbook.App.Views.Search;

namespace MyCookbook.App;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(ProfileHome), typeof(ProfileHome));
        Routing.RegisterRoute(nameof(SearchHome), typeof(SearchHome));
        Routing.RegisterRoute(nameof(MyCookbookHome), typeof(MyCookbookHome));
        Routing.RegisterRoute(nameof(ShoppingListHome), typeof(ShoppingListHome));
        Routing.RegisterRoute(nameof(CalendarHome), typeof(CalendarHome));
        Routing.RegisterRoute(nameof(RecipePage), typeof(RecipePage));
    }
}