using CommunityToolkit.Mvvm.ComponentModel;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;
using System;

namespace MyCookbook.App.ViewModels;

public partial class RecipeIngredientViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _guid;

    [ObservableProperty]
    private string? _name;

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
    private string? _notes;

    public RecipeIngredientViewModel(
        RecipeIngredientModel model)
    {
        Guid = model.Guid;
        Name = model.Ingredient.Name;
        ImageUri = model.Ingredient.ImageUri;
        QuantityType = model.QuantityType;
        MinValue = model.MinValue;
        MaxValue = model.MaxValue;
        NumberValue = model.NumberValue;
        MeasurementUnit = model.MeasurementUnit;
        Notes = model.Notes;
    }
}