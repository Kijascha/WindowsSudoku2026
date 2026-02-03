using System.Globalization;
using System.Windows.Data;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Core.Converters;

public class EnumEqualsMultiConverter : IMultiValueConverter
{


    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return false;

        if (values[0] is SudokuCellColor selected &&
            values[1] is SudokuCellColor current)
        {
            return selected == current;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // Unchecked → nichts ändern
        return new object[] { Binding.DoNothing, Binding.DoNothing };
    }
}
