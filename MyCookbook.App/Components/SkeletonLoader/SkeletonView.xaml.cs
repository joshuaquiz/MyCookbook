using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Components.SkeletonLoader;

public partial class SkeletonView : ContentView
{
    public static readonly BindableProperty SkeletonHeightProperty =
        BindableProperty.Create(
            nameof(SkeletonHeight),
            typeof(double),
            typeof(SkeletonView),
            defaultValue: 20.0);

    public static readonly BindableProperty SkeletonWidthProperty =
        BindableProperty.Create(
            nameof(SkeletonWidth),
            typeof(double),
            typeof(SkeletonView),
            defaultValue: 100.0);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(
            nameof(CornerRadius),
            typeof(double),
            typeof(SkeletonView),
            defaultValue: 4.0);

    public double SkeletonHeight
    {
        get => (double)GetValue(SkeletonHeightProperty);
        set => SetValue(SkeletonHeightProperty, value);
    }

    public double SkeletonWidth
    {
        get => (double)GetValue(SkeletonWidthProperty);
        set => SetValue(SkeletonWidthProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public SkeletonView()
    {
        InitializeComponent();
        StartShimmerAnimation();
    }

    private async void StartShimmerAnimation()
    {
        while (true)
        {
            await ShimmerOverlay.FadeTo(0.2, 750, Easing.SinInOut);
            await ShimmerOverlay.FadeTo(0.5, 750, Easing.SinInOut);
        }
    }
}

