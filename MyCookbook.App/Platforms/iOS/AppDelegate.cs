using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MyCookbook.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}