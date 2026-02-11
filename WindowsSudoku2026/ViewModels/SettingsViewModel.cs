using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Settings;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.Essential;

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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDirty))]
    private CandidateHandlingMode _selectedCandidateConflictMode;
    public bool IsDirty =>
        DefaultNamingPrefix != _originalSettings.DefaultNamingPrefix ||
        SelectedCandidateConflictMode != _originalSettings.CandidateConflictMode;

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
        _selectedCandidateConflictMode = _userSettings.CurrentValue.CandidateConflictMode;
    }

    partial void OnDefaultNamingPrefixChanged(string value)
    {
        UpdateAllSettingsCommand.NotifyCanExecuteChanged();
    }
    partial void OnSelectedCandidateConflictModeChanged(CandidateHandlingMode value)
    {
        UpdateAllSettingsCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(IsDirty))]
    private void UpdateAllSettings()
    {
        var updatedSettings = new UserSettings
        {
            DefaultNamingPrefix = DefaultNamingPrefix,
            CandidateConflictMode = SelectedCandidateConflictMode,
            // ... alle weiteren Properties hier übernehmen
        };

        _settingsService.SaveSettings<UserSettings>(_appPaths.UserSettingsFile, "UserSettings", updatedSettings);

        _originalSettings = updatedSettings;

        OnPropertyChanged(nameof(IsDirty));
        UpdateAllSettingsCommand.NotifyCanExecuteChanged();
    }
    [RelayCommand]
    private void CloseSettings()
    {
        _navigationService.CloseSidePanel<SettingsViewModel>();
    }
}
