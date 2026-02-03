using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Core.Converters;


public class NotificationTypeToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            // Wir suchen die Ressource im aktuellen Kontext (UserControl oder App)
            return type switch
            {
                NotificationType.Success => Application.Current.FindResource("SuccessNotificationBrush"),
                NotificationType.Error => Application.Current.FindResource("ErrorNotificationBrush"),
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}
