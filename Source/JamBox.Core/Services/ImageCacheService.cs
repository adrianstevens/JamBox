using Avalonia.Media.Imaging;
using JamBox.Core.Services.Interfaces;
using System.Collections.Concurrent;

namespace JamBox.Core.Services;

public class ImageCacheService : IImageCacheService
{
    private readonly ConcurrentDictionary<string, Bitmap> _cache = new();
    private readonly ConcurrentQueue<string> _accessOrder = new();
    private readonly object _evictionLock = new();
    private readonly int _maxCacheSize;
    private readonly HttpClient _httpClient;

    public ImageCacheService(int maxCacheSize = 100)
    {
        _maxCacheSize = maxCacheSize;
        _httpClient = new HttpClient();
    }

    public async Task<Bitmap?> GetImageAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Check cache first
        if (_cache.TryGetValue(url, out var cachedBitmap))
        {
            return cachedBitmap;
        }

        try
        {
            // Download the image asynchronously
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0;

            var bitmap = new Bitmap(memoryStream);

            // Add to cache
            AddToCache(url, bitmap);

            return bitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image from URL '{url}': {ex.Message}");
            return null;
        }
    }

    private void AddToCache(string url, Bitmap bitmap)
    {
        // Add to cache if not already present
        if (_cache.TryAdd(url, bitmap))
        {
            _accessOrder.Enqueue(url);
            EvictIfNecessary();
        }
    }

    private void EvictIfNecessary()
    {
        lock (_evictionLock)
        {
            while (_cache.Count > _maxCacheSize && _accessOrder.TryDequeue(out var oldestKey))
            {
                if (_cache.TryRemove(oldestKey, out var removedBitmap))
                {
                    removedBitmap.Dispose();
                }
            }
        }
    }

    public void ClearCache()
    {
        lock (_evictionLock)
        {
            foreach (var bitmap in _cache.Values)
            {
                bitmap.Dispose();
            }
            _cache.Clear();
            while (_accessOrder.TryDequeue(out _)) { }
        }
    }
}
