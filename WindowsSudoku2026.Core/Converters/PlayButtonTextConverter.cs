using System.Globalization;
using System.Windows.Data;

namespace WindowsSudoku2026.Core.Converters;

public class PlayButtonTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = TimeSpan TimeSpent
        // values[1] = bool IsSolved
        // values[2] = bool HasStarted (z.B. TimeSpent > 0)

        if (values[0] is not long ticks || values[1] is not bool isSolved)
            return "Play";

        if (isSolved) return "View Result";

        var time = TimeSpan.FromTicks(ticks);
        string timeString = time.TotalSeconds > 0
            ? $"Continue ({time:mm\\:ss})"
            : "Play";

        return timeString;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => [Binding.DoNothing, Binding.DoNothing];
}
