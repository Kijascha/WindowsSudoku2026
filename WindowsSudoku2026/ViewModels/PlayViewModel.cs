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
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.DTO;
using WindowsSudoku2026.Messaging;
using WindowsSudoku2026.Services;
using WindowsSudoku2026.Settings;

namespace WindowsSudoku2026.ViewModels;

public partial class PlayViewModel : ViewModel, IRecipient<ColorPaletteChangedMessage>, IRecipient<PuzzleSelectedMessage>
{
    [ObservableProperty] private ModifierKeys _activeModifiers;
    [ObservableProperty] private InputActionType _selectedInputActionType;
    [ObservableProperty] private GameType _gameMode;
    [ObservableProperty] private Visibility _visibilityState;
    [ObservableProperty] private ColorPalette _activePalette;
    [ObservableProperty] private bool _isFilled;
    [ObservableProperty] private bool _isLocked;
    private INavigationService _navigationService;
    private InputActionType _baseInputActionType;
    private IColorPaletteRepository _colorPaletteService;
    private IOptionsMonitor<UserSettings> _userOptionsMonitor;

    public IGameServiceV2 GameServiceV2 { get; }

    public PlayViewModel(
        IGameServiceV2 gameServiceV2,
        INavigationService navigationService,
        IOptionsMonitor<UserSettings> userOptionsMonitor,
        IColorPaletteRepository colorPaletteService)
    {
        GameServiceV2 = gameServiceV2;
        _navigationService = navigationService;
        _userOptionsMonitor = userOptionsMonitor;
        _colorPaletteService = colorPaletteService;
        _activeModifiers = ModifierKeys.None;
        _selectedInputActionType = InputActionType.Digits;
        _gameMode = GameType.Play;
        _baseInputActionType = InputActionType.Digits;
        _visibilityState = Visibility.Collapsed;
        _activePalette = ColorPaletteFactory.CreateDefaultPalette();
        GameServiceV2.CurrentPuzzle?.ActivePalette = _activePalette;
        IsLocked = false;

        // 2. Auf Live-Änderungen abonnieren
        WeakReferenceMessenger.Default.Register<ColorPaletteChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<PuzzleSelectedMessage>(this);

        // Im Konstruktor oder einer Init-Methode des PlayViewModels
        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(NavigationService.CurrentViewModel))
            {
                if (_navigationService.CurrentViewModel is PlayViewModel)
                {
                    // Timer fortsetzen
                    if (GameServiceV2.CurrentPuzzle != null && !GameServiceV2.CurrentPuzzle.IsSolved)
                        GameServiceV2.Timer.Start(GameServiceV2.CurrentPuzzle?.TimeSpent ?? TimeSpan.Zero);
                }
                else
                {
                    // Timer pausieren und Zeit im Model sichern, wenn wir die View verlassen
                    GameServiceV2.Timer.Pause();
                    // Hier der Fire-and-Forget Aufruf
                    _ = SaveProgressAndHandleErrorsAsync();
                }
            }
        };

        _ = UpdateActivePalette(_userOptionsMonitor.CurrentValue.ActiveColorPaletteId);

    }

    public void Receive(PuzzleSelectedMessage message)
    {
        GameServiceV2.Timer.Pause();
        GameServiceV2.Timer.Reset();

        var newPuzzle = DtoMapper.MapFromDto(message.SelectedPuzzle);
        GameServiceV2.CurrentPuzzle = newPuzzle.Clone();

        GameServiceV2.Timer.Start(newPuzzle.TimeSpent);
    }
    // Separate Methode zur Kapselung der Asynchronität
    private async Task SaveProgressAndHandleErrorsAsync()
    {
        try
        {
            // Nutzt die neue Methode im GameService (siehe vorherige Antwort)
            await GameServiceV2.SyncAndSaveCurrentProgressAsync();
            var result = await GameServiceV2.GetAvailablePuzzlesAsync();
            if (result != null)
                WeakReferenceMessenger.Default.Send(new PuzzleStateUpdatedMessage([.. result]));
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
            GameServiceV2.CurrentPuzzle?.ActivePalette = palette;

            // Dann die Property setzen - triggert NotifyPropertyChanged
            ActivePalette = palette;
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
            var finished = await GameServiceV2.VerifyAndFinishPuzzle();

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
    private void GoBack() => _navigationService.GoBack();
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
        if (GameServiceV2.CurrentPuzzle != null && !GameServiceV2.CurrentPuzzle.IsSolved)
            GameServiceV2.PuzzleCommandService.SmartRemovalFromSelected(GameServiceV2.CurrentPuzzle, GameType.Play);
    }
    [RelayCommand]
    private async Task CheckSolvability()
    {
        if (GameServiceV2.IsPuzzleSolvable())
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
                GameServiceV2.PuzzleCommandService.UpdateDigits(GameServiceV2.CurrentPuzzle, parsedDigit);

                if (GameServiceV2.CurrentPuzzle == null)
                    IsFilled = false;
                else
                    IsFilled = InputActionHelper.IsFilledOnInputAction(GameServiceV2.CurrentPuzzle, SelectedInputActionType, GameMode);

                break;

            case InputActionType.CenterCandidates:
            case InputActionType.CornerCandidates:
                GameServiceV2.PuzzleCommandService.UpdateCandidates(GameServiceV2.CurrentPuzzle, parsedDigit, SelectedInputActionType);
                break;

            case InputActionType.Colors:
                GameServiceV2.PuzzleCommandService.UpdateColors(GameServiceV2.CurrentPuzzle, parsedDigit);
                break;
        }
    }
}
