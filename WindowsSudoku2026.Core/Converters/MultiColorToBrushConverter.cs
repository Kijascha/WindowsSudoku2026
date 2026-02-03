using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Utils.Colors;

namespace WindowsSudoku2026.Core.Converters;

public class MultiColorToBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is not SudokuCellColor key)
            return Brushes.Transparent;
        if (values[1] is not ColorPalette palette)
            return Brushes.Transparent;

        return ColorPaletteFactory.GetBrush(palette, key);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new[] { Binding.DoNothing, Binding.DoNothing };
    }
}
