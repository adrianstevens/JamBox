using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace JamBox.Core.Converters
{
    public class ConnectionStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.Contains("failed", StringComparison.OrdinalIgnoreCase) || status.Contains("error", StringComparison.OrdinalIgnoreCase))
                {
                    return Brushes.Red;
                }
                if (status.Contains("success", StringComparison.OrdinalIgnoreCase) || status.Contains("found", StringComparison.OrdinalIgnoreCase))
                {
                    return Brushes.LightGreen;
                }
                return Brushes.LightBlue; // Intermediate status
            }
            return Brushes.White; // Default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}