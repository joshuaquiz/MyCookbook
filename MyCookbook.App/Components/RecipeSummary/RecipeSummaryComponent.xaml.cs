using System;
using Microsoft.Maui.Controls;
using MyCookbook.App.Views;

namespace MyCookbook.App.Components.RecipeSummary;

public partial class RecipeSummaryComponent
{
    public static readonly BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item),
        typeof(RecipeSummaryViewModel),
        typeof(RecipeSummaryComponent));

    public RecipeSummaryViewModel Item
    {
        get => (RecipeSummaryViewModel)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public RecipeSummaryComponent()
    {
        InitializeComponent();
        BindingContext = Item;
    }

    private async void OnTapped(
        object? sender,
        TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(
                $"{nameof(RecipePage)}?{nameof(Guid)}={Item.Guid}");
            /*await Browser.Default.OpenAsync(
                Item.ItemUrl,
                BrowserLaunchMode.SystemPreferred);*/
        }
        catch (Exception ex)
        {
            // An unexpected error occurred. No browser may be installed on the device.
            Console.WriteLine(ex.Message);
        }
        /*await Shell.Current.GoToAsync(
            $"{nameof(RecipePage)}?{nameof(Guid)}={Item.Guid}");*/
    }
}