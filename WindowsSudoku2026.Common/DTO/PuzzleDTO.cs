using WindowsSudoku2026.Common.Attributes;

namespace WindowsSudoku2026.Common.DTO;

[Table("Puzzles")]
public class PuzzleDTO
{
    public int Id { get; set; } // das als Int und dann nach namen filtern Id-wird automatisch von der DB gesetzt
    public string? Name { get; set; }
    public byte[]? PreviewImage { get; set; }
    public string Digits { get; set; } = string.Empty;
    public string CornerCandidateBitmasks { get; set; } = string.Empty;
    public string CenterCandidateBitmasks { get; set; } = string.Empty;
    public string SolverCandidateBitmasks { get; set; } = string.Empty;
    public string IsGivenString { get; set; } = string.Empty;
    public string CellColors { get; set; } = string.Empty;
    public string SolutionString { get; set; } = string.Empty;
    public long TimeSpentTicks { get; set; } = 0;
    public bool IsSolved { get; set; }
}
