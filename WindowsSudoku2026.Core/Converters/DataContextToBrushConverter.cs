using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Utils.Colors;

namespace WindowsSudoku2026.Core.Converters;

public class DataContextToBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = string (Zahl)
        // values[1] = ObservableCollection<SudokuColor>
        // values[2] = ColorPaletteV2

        if (values.Length < 3 || values[0] is not string s || !int.TryParse(s, out int digit))
            return Brushes.Transparent;

        // Sicherstellen, dass hier der NEUE Typ geprüft wird (WindowsSudoku2026.Common.Models.ColorPalette?)
        if (values[2] is not ColorPalette palette)
        {
            return Brushes.Transparent;
        }

        // Nutze die Palette zum Zeichnen
        // Da values[1] (die Liste) im MultiBinding steht, 
        // feuert dieser Converter jedes Mal, wenn du in der Liste ein Element ersetzt.
        var brush = ColorPaletteFactory.GetBrush(palette, (SudokuCellColor)digit);

        return brush ?? Brushes.Transparent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new[] { Binding.DoNothing, Binding.DoNothing };
    }
}
