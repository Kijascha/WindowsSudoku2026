using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Utils;

namespace WindowsSudoku2026.Core.Converters;

public class ContentToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Button.Content ist bei dir ein String
        if (value is not string s)
            return Brushes.Transparent;

        if (!int.TryParse(s, out int digit))
            return Brushes.Transparent;

        if (digit < 1 || digit > 9)
            return Brushes.Transparent;

        var color = (SudokuCellColor)digit;
        return SudokuColorPalette.GetBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
