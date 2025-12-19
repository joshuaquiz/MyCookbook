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
        // Initialize with empty collection immediately for responsive UI
        Ingredients = [];
    }

    /// <summary>
    /// Call this method when the page appears to load data asynchronously
    /// </summary>
    public void InitializeAsync()
    {
        // Fire and forget - don't block the UI
        _ = GetIngredientsCommand.ExecuteAsync(null);
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