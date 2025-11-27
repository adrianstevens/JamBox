using Avalonia.Media.Imaging;
using JamBox.Core.Services.Interfaces;
using System.Collections.Concurrent;

namespace JamBox.Core.Services;

public class ImageCacheService : IImageCacheService, IDisposable
{
    private readonly ConcurrentDictionary<string, Bitmap> _cache = new();
    private readonly ConcurrentDictionary<string, byte> _accessOrder = new();
    private readonly object _evictionLock = new();
    private readonly int _maxCacheSize;
    private readonly HttpClient _httpClient;
    private bool _disposed;

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

            // Add to cache with thread-safe locking
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
        lock (_evictionLock)
        {
            // Check if already in cache (another thread may have added it)
            if (_cache.ContainsKey(url))
            {
                bitmap.Dispose();
                return;
            }

            // Add to cache
            _cache[url] = bitmap;
            _accessOrder[url] = 0;
            EvictIfNecessary();
        }
    }

    private void EvictIfNecessary()
    {
        // Already under lock from AddToCache
        while (_cache.Count > _maxCacheSize)
        {
            // Get first key to evict
            var keyToEvict = _accessOrder.Keys.FirstOrDefault();
            if (keyToEvict != null)
            {
                _accessOrder.TryRemove(keyToEvict, out _);
                if (_cache.TryRemove(keyToEvict, out var removedBitmap))
                {
                    removedBitmap.Dispose();
                }
            }
            else
            {
                break;
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
            _accessOrder.Clear();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _httpClient.Dispose();
            ClearCache();
        }

        _disposed = true;
    }
}
