using Microsoft.Maui.Controls;
using MyCookbook.App.Views;

namespace MyCookbook.App;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
        // Register detail pages that are navigated to from main pages
        Routing.RegisterRoute(nameof(RecipePage), typeof(RecipePage));
        Routing.RegisterRoute(nameof(ShareRecipePage), typeof(ShareRecipePage));
    }
}