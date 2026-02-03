using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Records;
using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.ViewModels;

public partial class NotificationViewModel : ViewModel, IRecipient<ShowNotificationMessage>
{
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private NotificationType _type;

    public NotificationViewModel()
    {
        // Registrieren für die Nachricht
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(ShowNotificationMessage message)
    {
        Message = message.Message;
        Type = message.Type;
    }
}
