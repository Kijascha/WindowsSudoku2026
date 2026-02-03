using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.ObjectModel;
using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Settings;

namespace WindowsSudoku2026.Messaging
{
    public class GenericSettingsChangedMessage<T>(T value) : ValueChangedMessage<T>(value) where T : class { }
    public class SettingsChangedMessage(UserSettings value) : ValueChangedMessage<UserSettings>(value) { }
    public class ColorPaletteChangedMessage(ColorPaletteDTOV2 value) : ValueChangedMessage<ColorPaletteDTOV2>(value) { }
    public class PuzzleStateUpdatedMessage(ObservableCollection<PuzzleDTO> value) : ValueChangedMessage<ObservableCollection<PuzzleDTO>>(value) { }
}
