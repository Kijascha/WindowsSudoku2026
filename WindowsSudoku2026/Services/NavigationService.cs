using CommunityToolkit.Mvvm.ComponentModel;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.Services;

public partial class NavigationService(IServiceProvider provider) : ObservableObject, INavigationService
{
    [ObservableProperty] private IViewModel? _currentViewModel;
    [ObservableProperty] private IViewModel? _currentSidePanelViewModel;
    [ObservableProperty] private IViewModel? _currentNotificationViewModel;

    // Der Stack speichert den Verlauf der ViewModels
    private readonly Stack<IViewModel> _history = new();

    // ObservableProperty, damit die UI (TopBar) den Button ein/ausblenden kann
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private int _historyCount;
    public bool CanGoBack => _history.Count > 0;

    public void NavigateTo<TViewModel>()
        where TViewModel : IViewModel
    {
        // Speichere das aktuelle ViewModel im Stack, bevor wir wechseln
        if (CurrentViewModel != null)
        {
            _history.Push(CurrentViewModel);
            HistoryCount = _history.Count;
        }

        CurrentViewModel = provider.GetAbstractFactory<TViewModel>().Create();
    }
    public void GoBack()
    {
        if (_history.TryPop(out var previousViewModel))
        {
            CurrentViewModel = previousViewModel;
            HistoryCount = _history.Count;
        }
    }

    public void OpenSidePanel<TViewModel>()
        where TViewModel : IViewModel
    {
        // Falls schon offen, nichts tun (oder neu laden, falls gewünscht)
        if (IsSidePanelActive<TViewModel>()) return;

        CurrentSidePanelViewModel = provider.GetAbstractFactory<TViewModel>().Create();
    }
    public void CloseSidePanel<TViewModel>()
        where TViewModel : IViewModel
    {
        if (IsSidePanelActive<TViewModel>())
            CurrentSidePanelViewModel = null;
    }
    public bool IsSidePanelActive<TViewModel>() where TViewModel : IViewModel
    {
        return CurrentSidePanelViewModel is TViewModel;
    }
    public void ShowNotification<TViewModel>() where TViewModel : IViewModel
        => CurrentNotificationViewModel = provider.GetAbstractFactory<TViewModel>().Create();
    public void HideNotification<TViewModel>() where TViewModel : IViewModel
    {
        if (CurrentNotificationViewModel is TViewModel)
        {
            CurrentNotificationViewModel = null;
        }
    }
}
