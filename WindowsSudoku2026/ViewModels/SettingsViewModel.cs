using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.Essential;
using WindowsSudoku2026.Settings;

namespace WindowsSudoku2026.ViewModels;

public partial class SettingsViewModel : ViewModel
{
    private readonly IOptionsMonitor<UserSettings> _userSettings;
    private readonly ISettingsService _settingsService;
    private readonly IAppPaths _appPaths;
    private readonly INavigationService _navigationService;

    // Wir halten die Originalwerte fest
    private UserSettings _originalSettings;

    // Options for bindings
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDirty))]
    private string _defaultNamingPrefix;
    public bool IsDirty =>
        DefaultNamingPrefix != _originalSettings.DefaultNamingPrefix;
    public SettingsViewModel(
        IAppPaths appPaths,
        IOptionsMonitor<UserSettings> userSettings,
        ISettingsService settingsService,
        INavigationService navigationService)
    {
        _appPaths = appPaths;
        _userSettings = userSettings;
        _settingsService = settingsService;
        _navigationService = navigationService;
        _originalSettings = _userSettings.CurrentValue;

        _defaultNamingPrefix = _userSettings.CurrentValue.DefaultNamingPrefix;
    }

    partial void OnDefaultNamingPrefixChanged(string value)
    {

        UpdateAllSettingsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(IsDirty))]
    private void UpdateAllSettings()
    {
        // 1. Neues Settings-Objekt zusammenbauen
        var updatedSettings = new UserSettings
        {
            DefaultNamingPrefix = DefaultNamingPrefix,
            // ... alle weiteren Properties hier übernehmen
        };

        // 2. Physisch speichern
        //_settingsService.SaveSettings(updatedSettings);
        _settingsService.SaveSettings<UserSettings>(_appPaths.UserSettingsFile, "UserSettings", updatedSettings);

        // 3. Den neuen Stand als "Original" festlegen -> Button verschwindet
        _originalSettings = updatedSettings;

        // UI-Update erzwingen
        OnPropertyChanged(nameof(IsDirty));
        UpdateAllSettingsCommand.NotifyCanExecuteChanged();
    }
    [RelayCommand]
    private void CloseSettings()
    {
        _navigationService.CloseSidePanel<SettingsViewModel>();
    }
}
