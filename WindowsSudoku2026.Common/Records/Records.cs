using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Common.Records;


public record ShowNotificationMessage(string Message, NotificationType Type);
public record PuzzleDatabaseChangedMessage();
public record PuzzleSelectedMessage(PuzzleDTO SelectedPuzzle);
public record NamingOptions(bool UseAutoNaming, bool AlwaysAppendAutoSuffix, string DefaultPrefix, NamingStrategy PreferredStrategy);
