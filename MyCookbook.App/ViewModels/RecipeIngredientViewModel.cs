using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Enums;

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
    private string? _quantity;

    [ObservableProperty]
    private string? _generatedQuantity;

    [ObservableProperty]
    private MeasurementUnit _measurementUnit;

    [ObservableProperty]
    private string? _notes;

    public RecipeIngredientViewModel(
        RecipeIngredientModel model)
    {
        Guid = model.Guid;
        if (model.Ingredient.HasValue)
        {
            Name = model.Ingredient.Value.Name;
            ImageUri = model.Ingredient.Value.ImageUri;
        }

        Quantity = GeneratedQuantity = model.Quantity;
        MeasurementUnit = model.MeasurementUnit;
        Notes = model.Notes;
    }
}