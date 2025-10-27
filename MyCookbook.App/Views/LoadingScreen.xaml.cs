using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class LoadingScreen
{
    public LoadingScreen(
        LoadingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

