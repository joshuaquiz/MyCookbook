using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class ShareRecipePage : ContentPage
{
    public ShareRecipePage(ShareRecipeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ShareRecipeViewModel viewModel)
        {
            await viewModel.Initialize();
        }
    }
}

