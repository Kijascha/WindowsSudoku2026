using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Common.Utils;

public static class CandidateManager
{
    private static readonly HashSet<(int Row, int Column, int Candidate)> _removedSolverCandidates = new();
    private static readonly HashSet<(int Row, int Column, int Candidate)> _removedCenterCandidates = new();
    private static readonly HashSet<(int Row, int Column, int Candidate)> _removedCornerCandidates = new();
    private static readonly Dictionary<(int Row, int Column), int> _savedCandidates = [];
    private static readonly Dictionary<(int Row, int Column), (int center, int corner, int solver)> _savedCandidatesSeperate = [];
    public static bool RemoveCandidatesInUnit(IPuzzle currentPuzzle, UnitType unitType, CandidateType candidateType, int unitIndex, (int row, int column) currentCell, int candidate)
    {
        var unit = unitType switch
        {
            UnitType.Row => currentPuzzle.GetRowSpan(unitIndex),
            UnitType.Column => currentPuzzle.GetColumnSpan(unitIndex),
            UnitType.Box => currentPuzzle.GetBoxSpan(unitIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), "Invalid unit type")
        };

        bool anyRemoved = false;

        // handle other cells than the current cell
        foreach (var cell in unit)
        {
            if (cell.Digit != 0) continue; // Only consider empty cells
            bool removed = false;
            switch (candidateType)
            {
                case CandidateType.CenterCandidates:
                    removed = cell.CenterCandidates.Remove(candidate);

                    if (removed)
                    {
                        anyRemoved = true;
                        if (!_removedCenterCandidates.Contains((cell.Row, cell.Column, candidate)))
                        {
                            _removedCenterCandidates.Add((cell.Row, cell.Column, candidate));
                        }
                    }

                    if (currentPuzzle[cell.Row, cell.Column].CenterCandidates.Any() &&
                        currentPuzzle[cell.Row, cell.Column].CenterCandidates.Contains(candidate) &&
                        !_removedCenterCandidates.Contains((cell.Row, cell.Column, candidate)))
                    {
                        currentPuzzle[cell.Row, cell.Column].CenterCandidates.Remove(candidate);
                        _removedCenterCandidates.Add((cell.Row, cell.Column, candidate));
                    }
                    break;
                case CandidateType.CornerCandidates:
                    removed = cell.CornerCandidates.Remove(candidate);

                    if (removed)
                    {
                        anyRemoved = true;
                        if (!_removedCornerCandidates.Contains((cell.Row, cell.Column, candidate)))
                        {
                            _removedCornerCandidates.Add((cell.Row, cell.Column, candidate));
                        }
                    }

                    if (currentPuzzle[cell.Row, cell.Column].CornerCandidates.Any() &&
                        currentPuzzle[cell.Row, cell.Column].CornerCandidates.Contains(candidate) &&
                        !_removedCornerCandidates.Contains((cell.Row, cell.Column, candidate)))
                    {
                        currentPuzzle[cell.Row, cell.Column].CornerCandidates.Remove(candidate);
                        _removedCornerCandidates.Add((cell.Row, cell.Column, candidate));
                    }
                    break;
                case CandidateType.SolverCandidates:
                    removed = cell.SolverCandidates.Remove(candidate);

                    if (removed)
                    {
                        anyRemoved = true;
                        if (!_removedSolverCandidates.Contains((cell.Row, cell.Column, candidate)))
                        {
                            _removedSolverCandidates.Add((cell.Row, cell.Column, candidate));
                        }
                    }

                    if (currentPuzzle[cell.Row, cell.Column].SolverCandidates.Any() &&
                        currentPuzzle[cell.Row, cell.Column].SolverCandidates.Contains(candidate) &&
                        !_removedSolverCandidates.Contains((cell.Row, cell.Column, candidate)))
                    {
                        currentPuzzle[cell.Row, cell.Column].SolverCandidates.Remove(candidate);
                        _removedSolverCandidates.Add((cell.Row, cell.Column, candidate));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(candidateType), "Invalid candidate type");
            }
        }
        return anyRemoved;
    }
    public static void ClearAllCandidatesInCell(IPuzzle puzzle, int row, int column)
    {
        if (_savedCandidatesSeperate.ContainsKey((row, column)) == false)
        {
            _savedCandidatesSeperate[(row, column)] = (puzzle[row, column].CenterCandidates.BitMask,
                                                       puzzle[row, column].CornerCandidates.BitMask,
                                                       puzzle[row, column].SolverCandidates.BitMask);
        }

        puzzle[row, column].CenterCandidates.Clear();
        puzzle[row, column].CornerCandidates.Clear();
        puzzle[row, column].SolverCandidates.Clear();
    }

    public static bool RestoreCandidates(IPuzzle puzzle, CandidateType candidateType, int row, int column, int candidate)
    {
        RestoreUnitCandidates(puzzle, candidateType, row, column, candidate);

        if (puzzle[row, column].Digit != 0) return false;

        // Restore candidates for the current cell from _savedCandidatesSeperate, if present
        if (_savedCandidatesSeperate.TryGetValue((row, column), out (int center, int corner, int solver) savedBitmasks))
        {
            //TODO: Check if the readded Candidates are valid before readding them
            puzzle[row, column].CornerCandidates = new Candidates(savedBitmasks.corner);
            puzzle[row, column].CenterCandidates = new Candidates(savedBitmasks.center);
            puzzle[row, column].SolverCandidates = new Candidates(savedBitmasks.solver);
            _savedCandidatesSeperate.Remove((row, column));
        }

        return true;
    }
    private static void RestoreUnitCandidates(IPuzzle puzzle, CandidateType candidateType, int row, int column, int candidate)
    {
        IEnumerable<(int Row, int Column, int Candidate)> candidatesToRestore = candidateType switch
        {
            CandidateType.CenterCandidates => _removedCenterCandidates.Where(x => IsInSameUnit(x.Row, x.Column, row, column) && x.Candidate == candidate),
            CandidateType.CornerCandidates => _removedCornerCandidates.Where(x => IsInSameUnit(x.Row, x.Column, row, column) && x.Candidate == candidate),
            CandidateType.SolverCandidates => _removedSolverCandidates.Where(x => IsInSameUnit(x.Row, x.Column, row, column) && x.Candidate == candidate),
            _ => throw new ArgumentOutOfRangeException(nameof(candidateType), "Invalid candidate type")
        };
        foreach (var (r, c, cand) in candidatesToRestore)
        {
            if (puzzle.IsValidDigit(r, c, cand))
            {
                switch (candidateType)
                {
                    case CandidateType.CenterCandidates:
                        puzzle[r, c].CenterCandidates.Add(candidate);
                        _removedCenterCandidates.Remove((r, c, candidate));
                        break;
                    case CandidateType.CornerCandidates:
                        puzzle[r, c].CornerCandidates.Add(candidate);
                        _removedCornerCandidates.Remove((r, c, candidate));
                        break;
                    case CandidateType.SolverCandidates:
                        puzzle[r, c].SolverCandidates.Add(candidate);
                        _removedSolverCandidates.Remove((r, c, candidate));
                        break;
                }
            }
        }
    }
    public static bool IsInSameUnit(int row1, int col1, int row2, int col2)
    {
        return row1 == row2 ||
                col1 == col2 ||
                (row1 / 3) * 3 + (col1 / 3) == (row2 / 3) * 3 + (col2 / 3);
    }

}
