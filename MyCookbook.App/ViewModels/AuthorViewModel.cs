using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class AuthorViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _guid;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _bio;

    [ObservableProperty]
    private string? _location;

    [ObservableProperty]
    private Uri? _profileImageUri;

    [ObservableProperty]
    private Uri? _backgroundImageUri;

    public AuthorViewModel(AuthorModel model)
    {
        Guid = model.Guid;
        Name = model.Name;
        Bio = model.Bio;
        Location = model.Location;
        ProfileImageUri = model.ProfileImageUri;
        BackgroundImageUri = model.BackgroundImageUri;
    }
}

