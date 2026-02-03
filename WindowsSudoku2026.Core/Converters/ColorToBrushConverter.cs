using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WindowsSudoku2026.Core.Converters;

public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color c)
            return new SolidColorBrush(c);

        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush b)
            return b.Color;

        return System.Windows.Media.Colors.Transparent;
    }
}
