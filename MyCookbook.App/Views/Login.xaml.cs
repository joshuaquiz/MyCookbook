using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class Login
{
    public LoginViewModel ViewModel { get; set; }

    public Login(LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
    }
}