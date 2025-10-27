using Microsoft.Maui.Controls;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;

namespace MyCookbook.App.Views.MyCookbook;

public partial class CookbookRecipeItem
{
    public static BindableProperty ItemProperty = BindableProperty.Create(
        nameof(Item),
        typeof(Recipe),
        typeof(CookbookRecipeItem));

    public RecipeModel Item
    {
        get => (RecipeModel)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public CookbookRecipeItem()
    {
        InitializeComponent();
    }
}