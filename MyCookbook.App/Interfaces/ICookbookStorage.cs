using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MyCookbook.Common;

namespace MyCookbook.App.Interfaces;

public interface ICookbookStorage
{
    public ValueTask<AppTheme> GetCurrentAppTheme(Application application);

    public Task SetAppTheme(AppTheme appTheme, Application application);

    public Task Empty();

    public Task SetUser(UserProfile user);

    public ValueTask<UserProfile?> GetUser();
}