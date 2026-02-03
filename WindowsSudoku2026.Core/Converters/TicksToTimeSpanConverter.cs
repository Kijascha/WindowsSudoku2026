using System.Globalization;
using System.Windows.Data;

namespace WindowsSudoku2026.Core.Converters;

public class TicksToTimeSpanConverter : IValueConverter
{
    public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
    {
        if (value is long ticks)
        {
            return TimeSpan.FromTicks(ticks).ToString(@"hh\:mm\:ss");
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
