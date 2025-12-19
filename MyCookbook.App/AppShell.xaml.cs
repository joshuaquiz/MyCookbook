using Microsoft.Maui.Controls;
using MyCookbook.App.Views;

namespace MyCookbook.App;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        Routing.RegisterRoute(nameof(AuthorHome), typeof(AuthorHome));
        Routing.RegisterRoute(nameof(MyCookbookHome), typeof(MyCookbookHome));
        Routing.RegisterRoute(nameof(ShoppingListHome), typeof(ShoppingListHome));
        Routing.RegisterRoute(nameof(CalendarHome), typeof(CalendarHome));
        Routing.RegisterRoute(nameof(RecipePage), typeof(RecipePage));
        Routing.RegisterRoute(nameof(ShareRecipePage), typeof(ShareRecipePage));
    }
}