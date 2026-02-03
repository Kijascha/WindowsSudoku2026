using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Records;
using WindowsSudoku2026.Common.Utils.Colors;
using WindowsSudoku2026.Core.Helpers;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.DTO;
using WindowsSudoku2026.Messaging;
using WindowsSudoku2026.Services;
using WindowsSudoku2026.Settings;

namespace WindowsSudoku2026.ViewModels;

public partial class PlayViewModel : ViewModel, IRecipient<ColorPaletteChangedMessage>
{

    [ObservableProperty] IGameService _gameService;
    [ObservableProperty] private ModifierKeys _activeModifiers;
    [ObservableProperty] private InputActionType _selectedInputActionType;
    [ObservableProperty] private GameType _gameMode;
    [ObservableProperty] private Visibility _visibilityState;
    [ObservableProperty] private ColorPalette _activePalette;
    [ObservableProperty] private bool _isFilled;
    [ObservableProperty] private bool _isLocked;
    private INavigationService _navigationService;
    private InputActionType _baseInputActionType;
    private IColorPaletteService _colorPaletteService;
    private IOptionsMonitor<UserSettings> _userOptionsMonitor;

    public PlayViewModel(
        IGameService gameService,
        INavigationService navigationService,
        IOptionsMonitor<UserSettings> userOptionsMonitor,
        IColorPaletteService colorPaletteService)
    {
        _gameService = gameService;
        _navigationService = navigationService;
        _userOptionsMonitor = userOptionsMonitor;
        _colorPaletteService = colorPaletteService;
        _activeModifiers = ModifierKeys.None;
        _selectedInputActionType = InputActionType.Digits;
        _gameMode = GameType.Play;
        _baseInputActionType = InputActionType.Digits;
        _visibilityState = Visibility.Collapsed;
        _activePalette = ColorPaletteFactory.CreateDefaultPalette();
        _gameService.CurrentPuzzle.ActivePalette = _activePalette;
        IsLocked = false;

        // 2. Auf Live-Änderungen abonnieren
        WeakReferenceMessenger.Default.Register<ColorPaletteChangedMessage>(this);

        // Im Konstruktor oder einer Init-Methode des PlayViewModels
        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(NavigationService.CurrentViewModel))
            {
                if (_navigationService.CurrentViewModel is PlayViewModel)
                {
                    // Timer fortsetzen
                    if (!GameService.CurrentPuzzle.IsSolved)
                        GameService.Timer.Start(GameService.CurrentPuzzle?.TimeSpent ?? TimeSpan.Zero);
                }
                else
                {
                    // Timer pausieren und Zeit im Model sichern, wenn wir die View verlassen
                    GameService.Timer.Pause();
                    // Hier der Fire-and-Forget Aufruf
                    _ = SaveProgressAndHandleErrorsAsync();
                }
            }
        };

        _ = UpdateActivePalette(_userOptionsMonitor.CurrentValue.ActiveColorPaletteId);

    }
    // Separate Methode zur Kapselung der Asynchronität
    private async Task SaveProgressAndHandleErrorsAsync()
    {
        try
        {
            // Nutzt die neue Methode im GameService (siehe vorherige Antwort)
            await GameService.SaveCurrentPuzzleProgressAsync();
            var result = await GameService.GetAvailablePuzzlesAsync();
            if (result != null)
                WeakReferenceMessenger.Default.Send(new PuzzleStateUpdatedMessage(result));
        }
        catch (Exception ex)
        {
            // Wichtig: Logging, da wir uns in einem "vergessenen" Task befinden
            Debug.WriteLine($"Kritischer Fehler beim Hintergrund-Save: {ex.Message}");
            // Optional: User-Benachrichtigung via Messaging-Service
        }
    }

    public void Receive(ColorPaletteChangedMessage message)
    {
        _ = UpdateActivePalette(message.Value.Id);
    }
    private async Task UpdateActivePalette(int id)
    {
        var result = await _colorPaletteService.GetPaletteDtoById(id);
        if (result != null)
        {
            ColorPalette? palette = DtoMapper.MapFromDto(result);

            // Erst das Modell im Hintergrunddienst setzen
            GameService.CurrentPuzzle.ActivePalette = palette;

            // Dann die Property setzen - triggert NotifyPropertyChanged
            ActivePalette = palette;

            // WICHTIG: Manchmal braucht das MultiBinding einen "Anstoß" für den DataContext
            //OnPropertyChanged(nameof(ActivePalette));
        }
    }

    partial void OnIsFilledChanged(bool value)
    {
        _ = HandleFinishedPuzzle(value);
    }
    private async Task HandleFinishedPuzzle(bool isFilled)
    {

        if (isFilled)
        {
            var finished = await GameService.VerifyAndFinishPuzzle();

            if (finished)
            {
                IsLocked = true;
                IsFilled = false;

                await PopupNotification("Congratulations, you solved the puzzle!", NotificationType.Success);
            }
            else
            {
                await PopupNotification("Your puzzle has some issues!", NotificationType.Error);
            }
        }
    }

    private async Task PopupNotification(string message, NotificationType notificationType)
    {
        // Statt MessageBox einfach das Overlay öffnen
        _navigationService.ShowNotification<NotificationViewModel>();
        // 2. Die Nachricht senden (das neue ViewModel hört bereits zu)
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(message, notificationType));

        await Task.Delay(2_000);

        _navigationService.HideNotification<NotificationViewModel>();
    }
    partial void OnActiveModifiersChanged(ModifierKeys value)
    {
        switch (value)
        {
            case ModifierKeys.Control:
                SelectedInputActionType = InputActionType.CenterCandidates;
                break;
            case ModifierKeys.Shift:
                SelectedInputActionType = InputActionType.CornerCandidates;
                break;
            case ModifierKeys.Control | ModifierKeys.Shift:
                SelectedInputActionType = InputActionType.Colors;
                break;
            default:
                SelectedInputActionType = _baseInputActionType;
                break;
        }
    }
    [RelayCommand]
    private void GoBack() => _navigationService.NavigateTo<ChooseCustomPuzzleViewModel>();
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
    private void SelectInputAction(InputActionType type)
    {
        _baseInputActionType = type;
    }
    [RelayCommand]
    private void UpdateRemovals()
    {
        if (!GameService.CurrentPuzzle.IsSolved)
            GameService.SmartRemovalFromSelected();
    }
    [RelayCommand]
    private async Task CheckSolvability()
    {
        if (GameService.IsPuzzleSolvable())
        {
            await PopupNotification("Everything looks fine so far!", NotificationType.Success);
        }
        else
        {
            await PopupNotification("Something is wrong!", NotificationType.Error);
        }
    }
    [RelayCommand]
    private void UpdateButton(string digit)
    {
        if (IsLocked) return;
        if (!int.TryParse(digit, out int parsedDigit)) return;

        switch (SelectedInputActionType)
        {
            case InputActionType.Digits:
                GameService.UpdateDigits(parsedDigit);
                IsFilled = InputActionHelper.IsFilledOnInputAction(GameService.CurrentPuzzle, SelectedInputActionType, GameMode);
                break;

            case InputActionType.CenterCandidates:
            case InputActionType.CornerCandidates:
                GameService.UpdateCandidates(parsedDigit, SelectedInputActionType);
                break;

            case InputActionType.Colors:
                GameService.UpdateColors(parsedDigit);
                break;
        }
    }
}
