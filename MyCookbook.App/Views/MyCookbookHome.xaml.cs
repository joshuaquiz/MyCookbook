using Microsoft.Maui.Controls;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class MyCookbookHome
{
    private MyCookbookViewModel ViewModel { get; set; }
    private bool _hasInitialized;

    public MyCookbookHome(
        MyCookbookViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Load data asynchronously on first appearance
        if (!_hasInitialized)
        {
            _hasInitialized = true;
            ViewModel.InitializeAsync();
        }
    }
}