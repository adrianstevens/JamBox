using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System.Globalization;

namespace JamBox.Core.Converters;

public class UrlToBitmapConverter : IValueConverter
{
    private static readonly HttpClient _httpClient = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string url && !string.IsNullOrWhiteSpace(url))
        {
            try
            {
                // Synchronously download the image (not ideal for large images/UI thread, but works for demo)
                var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                using var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                return new Bitmap(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image from URL '{url}': {ex.Message}");
                return null;
            }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}