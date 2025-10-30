using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class PopularItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _guid;

    [ObservableProperty]
    private Uri? _imageUrl;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private Uri? _authorImageUrl;

    [ObservableProperty]
    private string _authorName = string.Empty;

    [ObservableProperty]
    private TimeSpan _totalTime;

    [ObservableProperty]
    private Uri _itemUrl;

    public PopularItemViewModel(PopularItem model)
    {
        Guid = model.Guid;
        ImageUrl = model.ImageUrl;
        Name = model.Name;
        AuthorImageUrl = model.AuthorImageUrl;
        AuthorName = model.AuthorName;
        TotalTime = model.TotalTime;
        ItemUrl = model.ItemUrl;
    }
}