using Avalonia.Data.Converters;
using System.Globalization;

namespace JamBox.Core.Converters;

public class BooleanInverseConverter : IValueConverter
{
    public static readonly BooleanInverseConverter Instance = new BooleanInverseConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return Avalonia.Data.BindingOperations.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}