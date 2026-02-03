using CommunityToolkit.Mvvm.Input;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.Services;

namespace WindowsSudoku2026.ViewModels;

public partial class MenuViewModel : ViewModel
{
    INavigationService _navigationService;
    public MenuViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
    [RelayCommand]
    private void GoToPlayMenu() => _navigationService.NavigateTo<PlayMenuViewModel>();
    [RelayCommand]
    private void GoToCreate() => _navigationService.NavigateTo<CreateViewModel>();
    [RelayCommand]
    private void GoToSettings()
    {
        if (_navigationService.IsSidePanelActive<SettingsViewModel>())
        {
            _navigationService.CloseSidePanel<SettingsViewModel>();
        }
        else
        {
            _navigationService.OpenSidePanel<SettingsViewModel>();
        }
    }
}
