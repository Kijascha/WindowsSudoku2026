using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Core.Services;

public static class SudokuValidationService
{
    public static void ValidateDigits(IPuzzle currentPuzzle)
    {
        ConflictAction(currentPuzzle, (cell) =>
        {
            currentPuzzle[cell.Row, cell.Column].IsConflicting = false;
        });

        ConflictAction(currentPuzzle, (cell) =>
        {
            if (!currentPuzzle.IsValidDigit(cell.Row, cell.Column, cell.Digit))
            {
                currentPuzzle[cell.Row, cell.Column].IsConflicting = true;
            }
        });
    }

    public static void ValidateCandidates(IPuzzle currentPuzzle)
    {
        ConflictAction(currentPuzzle, (cell) =>
        {
            currentPuzzle[cell.Row, cell.Column].ConflictedCandidates.Clear();

            for (int candidate = 1; candidate <= 9; candidate++)
            {
                if (!cell.CenterCandidates.Any() && !cell.CornerCandidates.Any()) continue;

                if (cell.CenterCandidates.Contains(candidate) || cell.CornerCandidates.Contains(candidate))
                {
                    if (!currentPuzzle.IsValidDigit(cell.Row, cell.Column, candidate))
                    {
                        currentPuzzle[cell.Row, cell.Column].ConflictedCandidates.Add(candidate);
                    }
                }
            }
        });
    }
    public static void ClearConflicts(IPuzzle currentPuzzle)
    {
        ConflictAction(currentPuzzle, (cell) =>
        {
            currentPuzzle[cell.Row, cell.Column].ConflictedCandidates.Clear();
        });
    }
    private static void ConflictAction(IPuzzle currentPuzzle, Action<Cell> conflictAction)
    {
        for (int r = 0; r < 9; r++)
        {
            foreach (var cell in currentPuzzle.GetRowSpan(r))
            {
                conflictAction(cell);
            }
        }
    }
}