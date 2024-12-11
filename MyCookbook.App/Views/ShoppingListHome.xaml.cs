using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class ShoppingListHome
{
    public ShoppingListViewModel ViewModel { get; set; }

    public ShoppingListHome(
        ShoppingListViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
    }
}