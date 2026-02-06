using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Common.Settings;

public class UserSettings
{
    public int ActiveColorPaletteId { get; set; } // <-- für das neue sqlite system
    public bool IsConflictCheckerEnabled { get; set; }
    public bool IsSeenCellsEnabled { get; set; }
    public bool IsDarkModeEnabled { get; set; }
    public bool AreSolverCandidatesVisible { get; set; }
    public string DefaultNamingPrefix { get; set; } = "Custom Puzzle ";
    public CandidateHandlingMode CandidateConflictMode { get; set; } = CandidateHandlingMode.AutoRemoval;

}
