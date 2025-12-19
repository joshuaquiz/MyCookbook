using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MyCookbook.App.Interfaces;

namespace MyCookbook.App.Controls;

public partial class CachedImage : ContentView
{
    private static IImageCacheService? _imageCacheService;
    private CancellationTokenSource? _loadCancellation;

    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(
            nameof(Source),
            typeof(Uri),
            typeof(CachedImage),
            defaultValue: null,
            propertyChanged: OnSourceChanged);

    public static readonly BindableProperty AspectProperty =
        BindableProperty.Create(
            nameof(Aspect),
            typeof(Aspect),
            typeof(CachedImage),
            defaultValue: Aspect.AspectFill);

    public static readonly BindableProperty CachedSourceProperty =
        BindableProperty.Create(
            nameof(CachedSource),
            typeof(ImageSource),
            typeof(CachedImage),
            defaultValue: null);

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(
            nameof(IsLoading),
            typeof(bool),
            typeof(CachedImage),
            defaultValue: true);

    public static readonly BindableProperty IsImageLoadedProperty =
        BindableProperty.Create(
            nameof(IsImageLoaded),
            typeof(bool),
            typeof(CachedImage),
            defaultValue: false);

    public Uri? Source
    {
        get => (Uri?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Aspect Aspect
    {
        get => (Aspect)GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
    }

    public ImageSource? CachedSource
    {
        get => (ImageSource?)GetValue(CachedSourceProperty);
        set => SetValue(CachedSourceProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsImageLoaded
    {
        get => (bool)GetValue(IsImageLoadedProperty);
        set => SetValue(IsImageLoadedProperty, value);
    }

    public CachedImage()
    {
        InitializeComponent();
    }

    public static void Initialize(IImageCacheService imageCacheService)
    {
        _imageCacheService = imageCacheService;
    }

    private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CachedImage cachedImage)
        {
            cachedImage.LoadImage();
        }
    }

    private async void LoadImage()
    {
        // Cancel any ongoing load
        _loadCancellation?.Cancel();
        _loadCancellation = new CancellationTokenSource();

        if (Source == null || _imageCacheService == null)
        {
            IsLoading = false;
            IsImageLoaded = false;
            CachedSource = null;
            return;
        }

        IsLoading = true;
        IsImageLoaded = false;

        try
        {
            var cachedPath = await _imageCacheService.GetCachedImagePathAsync(Source, _loadCancellation.Token);
            
            if (!string.IsNullOrEmpty(cachedPath) && !_loadCancellation.Token.IsCancellationRequested)
            {
                CachedSource = ImageSource.FromFile(cachedPath);
                IsImageLoaded = true;
            }
        }
        catch (OperationCanceledException)
        {
            // Image load was cancelled, ignore
        }
        catch (Exception)
        {
            // Failed to load image, show placeholder
            IsImageLoaded = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        // Reload image when handler changes (e.g., when view is recycled)
        if (Handler != null && Source != null)
        {
            LoadImage();
        }
    }
}

