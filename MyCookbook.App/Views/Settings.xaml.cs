using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class Settings
{
    public SettingsViewModel ViewModel { get; }

    public Settings(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();

        // Initialize async
        _ = ViewModel.InitializeAsync();

        // Add close command to ViewModel dynamically
        ViewModel.CloseCommand = new RelayCommand(async () => await CloseAsync());
    }
}