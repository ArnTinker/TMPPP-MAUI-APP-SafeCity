using System.Globalization;

namespace SafeCity.Converters;

/// <summary>
/// ConverterParameter = "TrueColorHex|FalseColorHex"
/// e.g. "#FFD60A|#9A9A9A"
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? "#FFD60A|#9A9A9A").Split('|');
        var hex = value is true ? parts[0] : (parts.Length > 1 ? parts[1] : "#FFFFFF");
        return Color.FromArgb(hex);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
