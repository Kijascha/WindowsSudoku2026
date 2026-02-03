using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WindowsSudoku2026.Core.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool isConditionMet) return Visibility.Collapsed;
        return isConditionMet ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
