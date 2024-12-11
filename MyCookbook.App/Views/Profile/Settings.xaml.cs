using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using MyCookbook.App.Interfaces;
using Application = Microsoft.Maui.Controls.Application;

namespace MyCookbook.App.Views.Profile;

public partial class Settings
{
    private readonly ICookbookStorage _cookbookStorage;

    public List<string> AppThemes { get; }

    public Settings(
        ICookbookStorage cookbookStorage)
    {
        _cookbookStorage = cookbookStorage;
        AppThemes =
        [
            nameof(AppTheme.Light),
            nameof(AppTheme.Dark),
            "System Default"
        ];
        BindingContext = this;
        InitializeComponent();
        ThemePicker.SelectedIndex = _cookbookStorage.GetCurrentAppTheme(Application.Current!).GetAwaiter().GetResult() switch
        {
            AppTheme.Light => 0,
            AppTheme.Dark => 1,
            _ => 2
        };
    }

    private async void Picker_OnSelectedIndexChanged(object? sender, EventArgs e) =>
        await _cookbookStorage.SetAppTheme(
            ThemePicker.SelectedIndex switch
            {
                0 => AppTheme.Light,
                1 => AppTheme.Dark,
                _ => AppTheme.Unspecified
            },
            Application.Current!);

    private async void Button_OnClicked(object? sender, EventArgs e) =>
        await CloseAsync();
}