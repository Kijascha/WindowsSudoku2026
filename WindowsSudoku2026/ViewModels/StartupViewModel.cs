using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.Services;

namespace WindowsSudoku2026.ViewModels;

public partial class StartupViewModel : ViewModel
{
    [ObservableProperty] private INavigationService _navigationService;
    [ObservableProperty] private IGameService _gameService;

    public bool CanTimeBeVisible
    {
        get => NavigationService.CurrentViewModel?.GetType() == typeof(PlayViewModel);
    }
    public StartupViewModel(INavigationService navigationService, IGameService gameService)
    {
        _navigationService = navigationService;
        _gameService = gameService;
        _navigationService.NavigateTo<MenuViewModel>();
    }

    [RelayCommand]
    private void GoBack()
    {
        NavigationService.GoBack();
    }

}
