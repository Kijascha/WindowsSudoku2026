using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.Services;

namespace WindowsSudoku2026.ViewModels;

public partial class CreateViewModel : ViewModel
{

    [ObservableProperty] IGameService _gameService;
    [ObservableProperty] private InputActionType _selectedInputActionType;
    [ObservableProperty] private GameType _gameMode;
    [ObservableProperty] private Visibility _visibilityState;

    private INavigationService _navigationService;

    public CreateViewModel(IGameService gameService, INavigationService navigationService)
    {
        _gameService = gameService;
        _navigationService = navigationService;
        _selectedInputActionType = InputActionType.Digits;
        _visibilityState = Visibility.Collapsed;
        _gameMode = GameType.Create;
        _gameService.CurrentPuzzle = new Puzzle();
    }
    [RelayCommand]
    private void GoBack() => _navigationService.NavigateTo<MenuViewModel>();

    [RelayCommand]
    private void ToggleSettings()
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
    [RelayCommand]
    private void ToggleSavePuzzleDialog()
    {
        if (_navigationService.IsSidePanelActive<SavePuzzleDialogViewModel>())
        {
            _navigationService.CloseSidePanel<SavePuzzleDialogViewModel>();
        }
        else
        {
            _navigationService.OpenSidePanel<SavePuzzleDialogViewModel>();
        }
    }
    [RelayCommand]
    private void UpdateRemovals()
    {
        GameService.SmartRemovalFromSelected();
    }
    [RelayCommand]
    private void UpdateButton(string digit)
    {
        if (!int.TryParse(digit, out int parsedDigit)) return;

        GameService.UpdateDigits(parsedDigit, GameType.Create);
    }

    [RelayCommand]
    private void Solve()
    {
    }
}
