using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class StepViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _guid;

    [ObservableProperty]
    private int _stepNumber;

    [ObservableProperty]
    private Uri? _imageUri;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private ObservableCollection<RecipeIngredientViewModel> _ingredients = [];

    public StepViewModel(StepModel model)
    {
        Guid = model.Guid;
        StepNumber = model.StepNumber;
        ImageUri = model.ImageUri;
        Description = model.Instructions;
        
        Ingredients.Clear();
        foreach (var ingredient in model.Ingredients)
        {
            Ingredients.Add(new RecipeIngredientViewModel(ingredient));
        }
    }
}