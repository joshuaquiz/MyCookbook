using Microsoft.Maui.Controls;
using MyCookbook.Common;

namespace MyCookbook.App.Views.MyCookbook;

public partial class CookbookRecipeItem
{
    public static BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item),
        typeof(Recipe),
        typeof(CookbookRecipeItem));

    public Recipe Item
    {
        get => (Recipe)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public CookbookRecipeItem()
    {
        InitializeComponent();
    }
}