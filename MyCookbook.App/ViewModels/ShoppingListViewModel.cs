using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class ShoppingListViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<IngredientModel>? _ingredients;

    public ShoppingListViewModel()
    {
        GetIngredientsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task GetIngredients()
    {
        IsBusy = true;
        await Task.Delay(TimeSpan.FromSeconds(1));
        Ingredients = new ObservableCollection<IngredientModel>([]);
        IsBusy = false;
    }
}