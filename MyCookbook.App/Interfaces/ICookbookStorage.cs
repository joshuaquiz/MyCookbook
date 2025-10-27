using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Interfaces;

public interface ICookbookStorage
{
    public ValueTask<AppTheme> GetCurrentAppTheme(Application application);

    public Task SetAppTheme(AppTheme appTheme, Application application);

    public Task Empty();

    public Task SetUser(UserProfileModel user);

    public ValueTask<UserProfileModel?> GetUser();
}