using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using JamBox.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JamBox.Core.Converters;

public static class AsyncImageLoader
{
    public static readonly AttachedProperty<string?> SourceProperty =
        AvaloniaProperty.RegisterAttached<Image, string?>("Source", typeof(AsyncImageLoader));

    static AsyncImageLoader()
    {
        SourceProperty.Changed.AddClassHandler<Image>(OnSourceChanged);
    }

    public static string? GetSource(Image image) => image.GetValue(SourceProperty);

    public static void SetSource(Image image, string? value) => image.SetValue(SourceProperty, value);

    private static async void OnSourceChanged(Image image, AvaloniaPropertyChangedEventArgs e)
    {
        var url = e.NewValue as string;

        if (string.IsNullOrWhiteSpace(url))
        {
            image.Source = null;
            return;
        }

        // Set source to null initially (placeholder behavior)
        image.Source = null;

        try
        {
            var cacheService = GetCacheService();
            if (cacheService != null)
            {
                var bitmap = await cacheService.GetImageAsync(url).ConfigureAwait(false);

                // Update the image on the UI thread
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Only update if the URL hasn't changed
                    if (GetSource(image) == url)
                    {
                        image.Source = bitmap;
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading async image from '{url}': {ex.Message}");
            // Leave source as null on error (shows nothing/placeholder)
        }
    }

    private static IImageCacheService? GetCacheService()
    {
        // Try to get the cache service from the application's service provider
        if (Application.Current is App app)
        {
            return app.Services?.GetService<IImageCacheService>();
        }
        return null;
    }
}
