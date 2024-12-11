using Microsoft.Maui.Controls;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views.MyCookbook;

public partial class MyCookbookHome
{
    private MyCookbookViewModel ViewModel { get; set; }

    public MyCookbookHome(
        MyCookbookViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
    }

    private void SearchBar_OnTextChanged(
        object? sender,
        TextChangedEventArgs e) =>
        ViewModel
            .SearchCommand
            .Execute(
                TextSearchBar.Text);
}