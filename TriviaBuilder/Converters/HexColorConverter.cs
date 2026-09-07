using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TriviaBuilder.Converters;

// Bridges the hex string stored in the model (matching trivia*.json) and the
// Avalonia.Media.Color the ColorPicker control works with.
public class HexColorConverter : IValueConverter
{
    public static readonly HexColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            try { return Color.Parse(s); }
            catch { return Colors.Gray; }
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color c) return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        return BindingOperations.DoNothing;
    }
}
