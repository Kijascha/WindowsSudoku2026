using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WindowsSudoku2026.ViewModels;

namespace WindowsSudoku2026.Essential.SpecialConverters
{
    internal class ViewModelToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is not PlayViewModel) ? Visibility.Hidden : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
