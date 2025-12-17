using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;

namespace MyCookbook.App.ViewModels;

public partial class ShoppingListItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _shoppingListItemId;

    [ObservableProperty]
    private Guid _authorId;

    [ObservableProperty]
    private Guid _ingredientId;

    [ObservableProperty]
    private Guid _recipeStepId;

    [ObservableProperty]
    private string? _ingredientName;

    [ObservableProperty]
    private Uri? _imageUri;

    [ObservableProperty]
    private QuantityType _quantityType;

    [ObservableProperty]
    private decimal? _minValue;

    [ObservableProperty]
    private decimal? _maxValue;

    [ObservableProperty]
    private decimal? _numberValue;

    [ObservableProperty]
    private MeasurementUnit _measurementUnit;

    [ObservableProperty]
    private decimal _multiplier = 1.0m;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isPurchased;

    [ObservableProperty]
    private DateTime _createdAt;
}

