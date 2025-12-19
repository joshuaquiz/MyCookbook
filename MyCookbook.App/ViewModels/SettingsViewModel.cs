using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MyCookbook.App.Interfaces;

namespace MyCookbook.App.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ICookbookStorage _cookbookStorage;

    [ObservableProperty]
    private int _selectedThemeIndex;

    public List<string> AppThemes { get; }

    // This will be set by the view since CloseAsync is a Popup method
    public IRelayCommand? CloseCommand { get; set; }

    public SettingsViewModel(ICookbookStorage cookbookStorage)
    {
        _cookbookStorage = cookbookStorage;
        AppThemes =
        [
            nameof(AppTheme.Light),
            nameof(AppTheme.Dark),
            "System Default"
        ];
    }

    public async Task InitializeAsync()
    {
        var currentTheme = await _cookbookStorage.GetCurrentAppTheme(Application.Current!);
        SelectedThemeIndex = currentTheme switch
        {
            AppTheme.Light => 0,
            AppTheme.Dark => 1,
            _ => 2
        };
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        _ = SetThemeAsync(value);
    }

    private async Task SetThemeAsync(int themeIndex)
    {
        var theme = themeIndex switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        await _cookbookStorage.SetAppTheme(theme, Application.Current!);
    }
}

