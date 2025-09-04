using Avalonia.Data.Converters;
using System.Globalization;

namespace JamBox.Core.Converters;

public class BooleanToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLoading)
        {
            return isLoading ? "Connecting..." : "Connect to Jellyfin";
        }
        return "Connect to Jellyfin"; // Default
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}