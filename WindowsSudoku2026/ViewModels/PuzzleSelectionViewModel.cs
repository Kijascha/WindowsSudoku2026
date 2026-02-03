using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Records;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.DTO;
using WindowsSudoku2026.Messaging;
using WindowsSudoku2026.Services;

namespace WindowsSudoku2026.ViewModels;

public partial class PuzzleSelectionViewModel : ViewModel, IRecipient<PuzzleDatabaseChangedMessage>, IRecipient<PuzzleStateUpdatedMessage>
{
    private readonly INavigationService _navigationService;
    [ObservableProperty] private IGameService _gameService;
    [ObservableProperty] private ObservableCollection<PuzzleDTO> _puzzles = [];
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(GoToPlayPuzzleCommand))] private PuzzleDTO? _selectedPuzzle; // Bindung an ListBox.SelectedItem
    [ObservableProperty] private BitmapSource? _selectedPreviewImage;

    public bool HasPuzzleProgress { get => SelectedPuzzle?.TimeSpentTicks > 0; }
    public PuzzleSelectionViewModel(INavigationService navigationService, IGameService gameService)
    {
        _navigationService = navigationService;
        _gameService = gameService;

        _selectedPreviewImage = null;

        WeakReferenceMessenger.Default.Register<PuzzleStateUpdatedMessage>(this);
        // Wir starten den asynchronen Prozess, ohne darauf zu blockieren
        _ = LoadPuzzlesAsync();
    }

    private async Task LoadPuzzlesAsync()
    {
        // Hole die Daten asynchron vom Service
        var loadedPuzzles = await GameService.GetAvailablePuzzlesAsync();

        // Da wir uns im ViewModel befinden, sorgt das PropertyChanged-Event 
        // der [ObservableProperty] dafür, dass die ListBox sich aktualisiert, 
        // sobald die Daten da sind.
        Puzzles = loadedPuzzles;
    }
    partial void OnSelectedPuzzleChanged(PuzzleDTO? value)
    {
        if (value != null)
            SelectedPreviewImage = DtoDataConverter.DecodeBitmapSourceFromBytes(value.PreviewImage);

        OnPropertyChanged(nameof(HasPuzzleProgress));
    }

    // Das Command, das die Navigation startet
    [RelayCommand(CanExecute = nameof(CanExecutePlay))]
    private void GoToPlayPuzzle()
    {
        if (SelectedPuzzle != null)
        {
            // Direktes Laden im Service
            WeakReferenceMessenger.Default.Send(new PuzzleSelectedMessage(SelectedPuzzle));
            _navigationService.NavigateTo<PlayViewModel>();
        }
    }
    // Die CanExecute-Methode stellt sicher, dass der Button nur bei Auswahl aktiv ist
    private bool CanExecutePlay() => SelectedPuzzle != null;

    [RelayCommand]
    private void ClearSelection()
    {
        // Setzt das ausgewählte Puzzle auf null. 
        // Da IsSelected an IsExpanded gebunden ist, klappt das Item automatisch zu.
        SelectedPuzzle = null;
    }
    [RelayCommand]
    private async Task ResetPuzzle()
    {
        if (SelectedPuzzle != null)
        {
            var resettedPuzzle = await GameService.ResetCurrentPuzzleAsync(DtoMapper.MapFromDto(SelectedPuzzle));

            if (resettedPuzzle == null) return;
            // Direktes Laden im Service
            WeakReferenceMessenger.Default.Send(new PuzzleSelectedMessage(resettedPuzzle));
            _navigationService.NavigateTo<PlayViewModel>();
        }
    }
    [RelayCommand]
    private async Task DeleteSelectedPuzzle()
    {
        if (SelectedPuzzle == null) return;

        // Sicherheitsabfrage (Optional, aber empfohlen)
        // Hier könntest du dein neues Notification-System oder eine einfache MessageBox nutzen

        try
        {
            // 1. Aus der Datenbank löschen (via generischer Methode)
            await GameService.DeletePuzzleAsync(SelectedPuzzle.Id);

            // 2. Aus der UI-Liste entfernen
            Puzzles.Remove(SelectedPuzzle);

            // 3. Auswahl zurücksetzen
            SelectedPuzzle = null;
            SelectedPreviewImage = null;

            await PopupNotification("Puzzle erfolgreich gelöscht.", NotificationType.Success);
        }
        catch (Exception ex)
        {
            await PopupNotification("Fehler beim Löschen: " + ex.Message, NotificationType.Error);
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

    public void Receive(PuzzleDatabaseChangedMessage message)
    {
        // Wenn die DB sich geändert hat -> Liste neu laden
        _ = LoadPuzzlesAsync();
    }

    public void Receive(PuzzleStateUpdatedMessage message)
    {
        Puzzles = message.Value;
    }
}
