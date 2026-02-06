using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Records;
using WindowsSudoku2026.Common.Settings;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.Messaging;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.Messaging;

namespace WindowsSudoku2026.ViewModels;

public partial class SavePuzzleDialogViewModel : ViewModel, IRecipient<GenericSettingsChangedMessage<UserSettings>>
{
    [ObservableProperty] private IGameServiceV2 _gameServiceV2;
    [ObservableProperty] private string _puzzleName;

    [ObservableProperty] private bool _useAutoNaming;
    [ObservableProperty] private bool _alwaysAppendAutoSuffix;
    [ObservableProperty] private string _defaultNamingPrefix;
    [ObservableProperty] private NamingStrategy _selectedNamingStrategy;
    [ObservableProperty] private NamingStrategy[] _namingStrategies;
    private readonly INavigationService _navigationService;
    private bool _isSaving;

    public SavePuzzleDialogViewModel(
        IGameServiceV2 gameServiceV2,
        IOptionsMonitor<UserSettings> settingsMonitor,
        INavigationService navigationService)
    {
        _gameServiceV2 = gameServiceV2;
        _navigationService = navigationService;
        _puzzleName = string.Empty;


        // Initialisiere mit den aktuellen Werten aus den UserSettings
        var current = settingsMonitor.CurrentValue;
        _useAutoNaming = false;
        _alwaysAppendAutoSuffix = false;
        _defaultNamingPrefix = current.DefaultNamingPrefix;
        _selectedNamingStrategy = NamingStrategy.Counter;
        _namingStrategies = Enum.GetValues<NamingStrategy>();
        _isSaving = false;

        // Registrierung für die Nachricht
        WeakReferenceMessenger.Default.Register(this);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SavePuzzle()
    {
        _isSaving = true;
        SavePuzzleCommand.NotifyCanExecuteChanged();

        try
        {
            // Wir fragen die View: "Kannst du mir bitte ein Bild schicken?"
            // Das ViewModel wartet hier asynchron, bis die View antwortet.
            var bitmap = await WeakReferenceMessenger.Default.Send<RequestCaptureMessage>();

            if (bitmap != null)
            {
                // Jetzt erst das Bild dem Puzzle zuweisen
                GameServiceV2.CurrentPuzzle?.PreviewImage = bitmap;
                GameServiceV2.CurrentPuzzle?.IsSolved = false;
                // Und jetzt sicher speichern
                var success = await GameServiceV2.CreateAndSaveNewPuzzle(UseAutoNaming, AlwaysAppendAutoSuffix, DefaultNamingPrefix, SelectedNamingStrategy);
                //var success = await GameService.SaveNewCustomPuzzleAsync(UseAutoNaming, AlwaysAppendAutoSuffix, DefaultNamingPrefix, SelectedNamingStrategy);

                // TODO: Eine Art von Benachrichtigugn dass das Puzzle erfolgreich gespeichert wurde.
                if (success)
                {
                    await PopupNotification("Erfolgreich gespeichert!", NotificationType.Success);
                }
                else
                {
                    await PopupNotification("Puzzle konnte nicht gespeichert werden!", NotificationType.Error);
                }
            }
        }
        catch (Exception ex)
        {
            await PopupNotification(ex.Message, NotificationType.Error);
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            _isSaving = false;
            SavePuzzleCommand.NotifyCanExecuteChanged();
        }

    }
    private bool CanSave() => !_isSaving;
    private async Task PopupNotification(string message, NotificationType notificationType)
    {
        // Statt MessageBox einfach das Overlay öffnen
        _navigationService.ShowNotification<NotificationViewModel>();
        // 2. Die Nachricht senden (das neue ViewModel hört bereits zu)
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(message, notificationType));

        await Task.Delay(2_000);

        _navigationService.HideNotification<NotificationViewModel>();
    }
    [RelayCommand]
    private void CloseSavePuzzleDialog()
    {
        _navigationService.CloseSidePanel<SavePuzzleDialogViewModel>();
    }

    public void Receive(GenericSettingsChangedMessage<UserSettings> message)
    {
        // Wichtig: UI-Updates müssen in WPF oft zurück in den Main-Thread
        App.Current.Dispatcher.Invoke(() =>
        {
            DefaultNamingPrefix = message.Value.DefaultNamingPrefix;
        });
    }
}
