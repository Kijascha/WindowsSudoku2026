using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WindowsSudoku2026.Common.DTO;

namespace WindowsSudoku2026.Core.Converters
{
    public class SelectedPuzzleToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is PuzzleDTO selectedPuzzle && values[1] is PuzzleDTO currentPuzzle)
            {
                if (selectedPuzzle.Id != currentPuzzle.Id) return Visibility.Hidden;
            }
            else
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
         => [Binding.DoNothing, Binding.DoNothing];
    }
}
