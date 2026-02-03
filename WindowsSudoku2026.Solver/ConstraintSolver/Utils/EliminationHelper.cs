using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Utils;

internal static class EliminationHelper
{
    public static bool SetDigit(IPuzzle puzzle, int row, int column, int digit)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));

        if (puzzle[row, column].Digit != 0)
        {
            return false;
        }

        puzzle[row, column].Digit = digit;

        RemoveSolverCandidatesInRelatedUnits(puzzle, row, column, digit);

        return true;
    }
    public static bool RemoveSolverCandidatesInRelatedUnits(IPuzzle puzzle, int row, int column, int digit)
    {
        int boxRow = row / 3;
        int boxCol = column / 3;
        int boxIndex = boxRow * 3 + boxCol;

        bool isRemoved = false;


        isRemoved |= RemoveCandidatesForSolverInUnit(puzzle, UnitType.Row, row, digit);
        isRemoved |= RemoveCandidatesForSolverInUnit(puzzle, UnitType.Column, column, digit);
        isRemoved |= RemoveCandidatesForSolverInUnit(puzzle, UnitType.Box, boxIndex, digit);

        return isRemoved; // reich wenn in mindestens einer unit etwas erfolgreich removed wurde
    }
    public static bool RemoveCandidatesForSolverInUnit(IPuzzle currentPuzzle, UnitType unitType, int unitIndex, int candidate)
    {

        var unit = unitType switch
        {
            UnitType.Row => currentPuzzle.GetRowSpan(unitIndex),
            UnitType.Column => currentPuzzle.GetColumnSpan(unitIndex),
            UnitType.Box => currentPuzzle.GetBoxSpan(unitIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), "Invalid unit type")
        };

        bool anyRemoved = false;

        foreach (var cell in unit)
        {
            if (cell.Digit != 0) continue; // Only consider empty cells

            if (cell.SolverCandidates.Remove(candidate))
            {
                anyRemoved = true;
            }
        }
        return anyRemoved;
    }
    public static bool IsUnsolvableCell(IPuzzle puzzle, int row, int column)
    {
        return (puzzle[row, column].Digit == 0 && puzzle[row, column].SolverCandidates.BitMask == 0);
    }
}
