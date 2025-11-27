using Avalonia.Media.Imaging;

namespace JamBox.Core.Services.Interfaces;

public interface IImageCacheService
{
    Task<Bitmap?> GetImageAsync(string url);
    void ClearCache();
}
