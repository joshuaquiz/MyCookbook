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
    private Task? _loadTask;

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
        _loadCancellation?.Dispose();
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

        // Add a small delay to debounce rapid source changes (e.g., during scrolling)
        // This prevents canceling downloads that are about to complete
        try
        {
            await Task.Delay(50, _loadCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            IsLoading = false;
            return;
        }

        // Store the current task so we can track it
        _loadTask = LoadImageAsync(_loadCancellation.Token);
        await _loadTask;
    }

    private async Task LoadImageAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cachedPath = await _imageCacheService!.GetCachedImagePathAsync(Source!, cancellationToken);

            if (!string.IsNullOrEmpty(cachedPath) && !cancellationToken.IsCancellationRequested)
            {
                CachedSource = ImageSource.FromFile(cachedPath);
                IsImageLoaded = true;
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                // Image load was cancelled, keep showing loading state
                return;
            }
        }
        catch (TaskCanceledException)
        {
            // Image load was cancelled, ignore
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
            if (!cancellationToken.IsCancellationRequested)
            {
                IsLoading = false;
            }
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

