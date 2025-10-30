using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _guid;

    [ObservableProperty]
    private Uri? _backgroundImageUri;

    [ObservableProperty]
    private Uri? _profileImageUri;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _country = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private int _recipesAdded;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private bool _isPremium;

    [ObservableProperty]
    private bool _isFollowed;

    [ObservableProperty]
    private ObservableCollection<PopularItemViewModel> _recentRecipes = [];

    public UserProfileViewModel(UserProfileModel model)
    {
        Guid = model.Guid;
        BackgroundImageUri = model.BackgroundImageUri;
        ProfileImageUri = model.ProfileImageUri;
        FirstName = model.FirstName;
        LastName = model.LastName;
        Country = model.Country;
        City = model.City;
        Age = model.Age;
        RecipesAdded = model.RecipesAdded;
        Description = model.Description;
        IsPremium = model.IsPremium;
        IsFollowed = model.IsFollowed;
        
        RecentRecipes.Clear();
        foreach (var item in model.RecentRecipes)
        {
            RecentRecipes.Add(new PopularItemViewModel(item));
        }
    }
}