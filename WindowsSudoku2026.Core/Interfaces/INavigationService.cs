using System.ComponentModel;
using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.Core.Interfaces;

public interface INavigationService : INotifyPropertyChanged
{
    IViewModel? CurrentViewModel { get; set; }
    IViewModel? CurrentSidePanelViewModel { get; set; }
    IViewModel? CurrentNotificationViewModel { get; set; }
    int HistoryCount { get; set; }
    bool CanGoBack { get; }
    void GoBack();
    void NavigateTo<TViewModel>() where TViewModel : IViewModel;
    void OpenSidePanel<TViewModel>() where TViewModel : IViewModel;
    void CloseSidePanel<TViewModel>() where TViewModel : IViewModel;
    bool IsSidePanelActive<TViewModel>() where TViewModel : IViewModel;
    void ShowNotification<TViewModel>() where TViewModel : IViewModel;
    void HideNotification<TViewModel>() where TViewModel : IViewModel;
}
