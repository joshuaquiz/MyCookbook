using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MyCookbook.App.Interfaces;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

[QueryProperty(nameof(UserProfile), nameof(UserProfile))]
[QueryProperty(nameof(Guid), nameof(Guid))]
public partial class ProfileViewModel(
    ICookbookStorage cookbookStorage)
    : BaseViewModel
{
    [ObservableProperty]
    private UserProfileModel? _userProfile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GetUserProfileCommand))]
    private string? _guid;

    [RelayCommand]
    private async Task GetUserProfile()
    {
        IsBusy = true;
        await Task.Delay(TimeSpan.FromSeconds(1));
        UserProfile = await cookbookStorage.GetUser();
        IsBusy = false;
    }

    public async Task Logout()
    {
        IsBusy = true;
        await Task.Delay(TimeSpan.FromSeconds(1));
        await cookbookStorage.Empty();
        IsBusy = false;
    }
}