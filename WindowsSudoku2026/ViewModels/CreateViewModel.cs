using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.ViewModels;

public partial class CreateViewModel : ViewModel
{
    [ObservableProperty] private IGameServiceV2 _gameServiceV2;
    [ObservableProperty] private InputActionType _selectedInputActionType;
    [ObservableProperty] private GameType _gameMode;
    [ObservableProperty] private Visibility _visibilityState;

    private INavigationService _navigationService;

    public IPuzzleCommandService CommandService { get; }

    public CreateViewModel(
        IGameServiceV2 gameServiceV2,
        IPuzzleCommandService commandService,
        IPuzzle puzzle,
        INavigationService navigationService)
    {
        _gameServiceV2 = gameServiceV2;
        CommandService = commandService;
        _navigationService = navigationService;
        _selectedInputActionType = InputActionType.Digits;
        _visibilityState = Visibility.Collapsed;
        _gameMode = GameType.Create;
        _gameServiceV2.CurrentPuzzle = puzzle;
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
        GameServiceV2.PuzzleCommandService.SmartRemovalFromSelected(GameServiceV2.CurrentPuzzle, GameType.Create);
    }
    [RelayCommand]
    private void UpdateButton(string digit)
    {
        if (!int.TryParse(digit, out int parsedDigit)) return;

        GameServiceV2.PuzzleCommandService.UpdateDigits(GameServiceV2.CurrentPuzzle, parsedDigit, GameType.Create);
    }

    [RelayCommand]
    private void Solve()
    {
    }
}
