using System.Globalization;

namespace SafeCity.Converters;

/// <summary>ConverterParameter = "TrueString|FalseString"</summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? "Yes|No").Split('|');
        return value is true ? parts[0] : (parts.Length > 1 ? parts[1] : "No");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
