using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.ViewModels;

public partial class StartupViewModel : ViewModel
{
    [ObservableProperty] private INavigationService _navigationService;
    [ObservableProperty] private IGameServiceV2 _gameService;

    public bool CanTimeBeVisible
    {
        get => NavigationService.CurrentViewModel?.GetType() == typeof(PlayViewModel);
    }
    public StartupViewModel(INavigationService navigationService, IGameServiceV2 gameService)
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
