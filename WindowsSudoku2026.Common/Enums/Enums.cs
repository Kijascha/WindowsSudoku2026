namespace WindowsSudoku2026.Common.Enums;

// All Enums in one File!

public enum NotificationType
{
    Success,
    Error
}
public enum NamingStrategy
{
    Counter,
    Timestamp
}
public enum SlideDirection
{
    Left,
    Right,
    Top,
    Bottom
}
public enum UnitType
{
    Row,
    Column,
    Box
}
public enum InputActionType
{
    Digits,
    CenterCandidates,
    CornerCandidates,
    Colors
}
public enum RemovalAction
{
    None,
    Digit,
    CornerCandidates,
    CenterCandidates,
    Colors
}
public enum GameType
{
    Create,
    Play
}
public enum CandidateType
{
    CenterCandidates,
    CornerCandidates,
    SolverCandidates
}
public enum SudokuCellColor
{
    None = 0,      // entspricht CellBackground
    Color1,
    Color2,
    Color3,
    Color4,
    Color5,
    Color6,
    Color7,
    Color8,
    Color9
}

public enum CandidateHandlingMode
{
    None,
    AutoRemoval,
    HighlightConflicts
}