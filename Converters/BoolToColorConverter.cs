using System.Globalization;

namespace SafeCity.Converters;

/// <summary>
/// ConverterParameter = "TrueColorHex|FalseColorHex"
/// e.g. "#34C759|#FF3B30"
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? "#FFFFFF|#FF3B30").Split('|');
        var hex = value is true ? parts[0] : (parts.Length > 1 ? parts[1] : "#FFFFFF");
        return Color.FromArgb(hex);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
