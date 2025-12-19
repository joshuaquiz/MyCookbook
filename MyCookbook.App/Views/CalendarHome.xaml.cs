using System.Threading.Tasks;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class CalendarHome
{
    private CalendarHomeViewModel ViewModel { get; }
    private bool _hasInitialized;

    public CalendarHome(CalendarHomeViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Initialize the ViewModel asynchronously on first appearance (fire and forget)
        if (!_hasInitialized)
        {
            _hasInitialized = true;
            _ = ViewModel.InitializeAsync();
        }
    }
}