using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Windows.Media;
using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Utils.Colors;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.DTO;
using WindowsSudoku2026.Essential;
using WindowsSudoku2026.Messaging;
using WindowsSudoku2026.Settings;

namespace WindowsSudoku2026.ViewModels;

public partial class ColorPaletteSelectionViewModel : ViewModel, IRecipient<ColorPickerMessage>
{
    private Color _receivedColor;

    private readonly IAppPaths _appPaths;
    private readonly IColorPaletteRepository _colorPaletteRepo;
    private readonly IOptionsMonitor<UserSettings> _optionsMonitor;
    private readonly ISettingsService _settingsService;

    [ObservableProperty] private ObservableCollection<ColorPaletteDTOV2> _availablePalettes;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(DeletePaletteCommand))] private ColorPaletteDTOV2? _selectedPalette;
    [ObservableProperty] private ColorPalette? _activeColorPalette;
    [ObservableProperty] private SudokuColor? _selectedColor;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(UpdatePaletteCommand))] private bool _isDirty;

    public ColorPaletteSelectionViewModel(
        IAppPaths appPaths,
        IColorPaletteRepository colorPaletteRepo,
        IOptionsMonitor<UserSettings> optionsMonitor,
        ISettingsService settingsService)
    {
        _appPaths = appPaths;
        _colorPaletteRepo = colorPaletteRepo;
        _optionsMonitor = optionsMonitor;
        _settingsService = settingsService;

        _receivedColor = Colors.White;
        _availablePalettes = [];

        WeakReferenceMessenger.Default.Register<ColorPickerMessage>(this);

        _ = InitializeAsync();
    }
    public void Receive(ColorPickerMessage message)
    {
        _receivedColor = message.newColor;
    }
    public async Task InitializeAsync()
    {
        await UpdateAvailablePalettes();

        SelectedPalette = AvailablePalettes.FirstOrDefault(p => p.Id == _optionsMonitor.CurrentValue.ActiveColorPaletteId);
        ActiveColorPalette = AvailablePalettes.Select(DtoMapper.MapFromDto).FirstOrDefault(p => p.Id == _optionsMonitor.CurrentValue.ActiveColorPaletteId) ?? ColorPaletteFactory.CreateDefaultPalette();
    }
    partial void OnSelectedPaletteChanged(ColorPaletteDTOV2? value)
    {
        if (value != null)
        {
            ActiveColorPalette = DtoMapper.MapFromDto(value);

            WeakReferenceMessenger.Default.Send<ColorPaletteChangedMessage>(new(value));
            // Da die ActivePaletteId ein "Einzelwert" in den Settings ist, 
            // nutzen wir hier den SettingsService
            _settingsService.UpdateSettings<UserSettings>(
                _appPaths.UserSettingsFile,
                "UserSettings", // Default falls Datei leer
                settings =>
                {
                    settings.ActiveColorPaletteId = value.Id;
                });
        }
    }
    private async Task UpdateAvailablePalettes()
    {
        var result = await _colorPaletteRepo.GetAllColorPaletteDtos();
        AvailablePalettes = [.. result];
    }
    [RelayCommand]
    private async Task NewPalette()
    {
        var result = await _colorPaletteRepo.SavePaletteDto(DtoMapper.MapToDto(ColorPaletteFactory.CreateDefaultPalette()));

        await UpdateAvailablePalettes();

        SelectedPalette = AvailablePalettes.LastOrDefault();
        ActiveColorPalette = AvailablePalettes.Select(DtoMapper.MapFromDto).LastOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdatePalette()
    {
        if (ActiveColorPalette == null) return;

        // 1. Speichern via Service
        await _colorPaletteRepo.UpdatePalette(DtoMapper.MapToDto(ActiveColorPalette));

        // 2. Zustand zurücksetzen
        IsDirty = false;

        await InitializeAsync();
    }

    private bool CanUpdate() => IsDirty && ActiveColorPalette != null;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeletePalette()
    {
        if (SelectedPalette == null) return;

        // 1. Löschen im Service
        await _colorPaletteRepo.DeletePalette(SelectedPalette.Id);

        // 2. Liste aktualisieren
        await UpdateAvailablePalettes();

        // 3. Neue Selektion festlegen
        // Wir versuchen die erste Palette zu nehmen, falls vorhanden
        var nextPalette = AvailablePalettes.FirstOrDefault();

        if (nextPalette != null)
        {
            SelectedPalette = nextPalette;
        }
        else
        {
            // Fallback: Wenn alles leer ist, neue Default anlegen (verhindert Null-Referenzen im Game)
            await NewPalette();
        }

        // InitializeAsync sorgt dafür, dass ActivePalette und Settings wieder stimmen
        await InitializeAsync();
    }

    // Man sollte nur löschen können, wenn mehr als eine Palette existiert (optional, aber sicher)
    private bool CanDelete() => SelectedPalette != null && AvailablePalettes.Count > 1;

    [RelayCommand]
    private void ApplyColor()
    {
        if (SelectedColor != null) UpdateColor(SelectedColor);
    }
    private void UpdateColor(SudokuColor selectedColor)
    {
        if (ActiveColorPalette == null) return;

        var index = ActiveColorPalette.SudokuColors.IndexOf(selectedColor);
        if (index != -1)
        {
            ActiveColorPalette.SudokuColors[index] = new SudokuColor(selectedColor.Key, _receivedColor);

            IsDirty = true;
        }
    }
}
