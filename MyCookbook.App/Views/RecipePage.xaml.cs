using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class RecipePage
{
    private RecipeViewModel ViewModel { get; set; }

    public RecipePage(
        RecipeViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }
}