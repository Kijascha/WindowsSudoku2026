using System.Globalization;
using System.Windows.Data;

namespace WindowsSudoku2026.Core.Converters;

public class BestTimeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = long TimeSpentTicks
        // values[1] = bool IsSolved

        if (values.Length >= 2 && values[0] is long ticks && values[1] is bool isSolved)
        {
            if (isSolved && ticks > 0)
            {
                var time = TimeSpan.FromTicks(ticks);
                // Format: Stunden:Minuten:Sekunden
                return time.ToString(@"hh\:mm\:ss");
            }
        }

        // Standardanzeige, wenn nicht gelöst oder Zeit 0
        return "--:--:--";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => [Binding.DoNothing, Binding.DoNothing];
}
