using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Utils;
using WindowsSudoku2026.Core.Interfaces;

namespace WindowsSudoku2026.Core.Services;

public class PuzzleCommandService : IPuzzleCommandService
{
    #region Public Update Methods
    public void UpdateColors(IPuzzle? currentPuzzle, int colorCode)
    {
        if (!AnyCellsSelected(currentPuzzle))
            return;

        var color = (SudokuCellColor)colorCode;

        UpdateSelectedCells(currentPuzzle, cell =>
        {
            if (cell.CellColors.Any(c => c.Equals(color)))
                cell.CellColors.Remove(color);
            else
                cell.CellColors.Add(color);
        });
    }
    public void UpdateCandidates(IPuzzle? currentPuzzle, int candidate, InputActionType selectedInputActionType)
    {
        if (candidate < 0 || candidate > IPuzzle.Size || !AnyCellsSelected(currentPuzzle))
            return;

        CandidateType type =
            selectedInputActionType == InputActionType.CenterCandidates ? CandidateType.CenterCandidates :
            selectedInputActionType == InputActionType.CornerCandidates ? CandidateType.CornerCandidates :
            throw new InvalidOperationException("No candidate modifier active");

        UpdateSelectedCells(currentPuzzle, cell =>
        {
            if (!cell.IsGiven)
                UpdateCandidate(currentPuzzle, cell.Row, cell.Column, candidate, type);
        });
    }
    public void UpdateDigits(IPuzzle? currentPuzzle, int digit, GameType gameType = GameType.Play)
    {
        if (digit < 0 || digit > IPuzzle.Size || !AnyCellsSelected(currentPuzzle))
            return;

        UpdateSelectedCells(currentPuzzle, cell =>
        {
            if (gameType == GameType.Play)
            {
                if (!cell.IsGiven)
                {
                    UpdateDigit(currentPuzzle, cell.Row, cell.Column, digit, gameType);
                }
            }
            else
            {
                UpdateDigit(currentPuzzle, cell.Row, cell.Column, digit, gameType);
            }
        });
    }
    public void RemoveDigits(IPuzzle? currentPuzzle, GameType gameType)
    {
        if (!AnyCellsSelected(currentPuzzle)) return;

        UpdateSelectedCells(currentPuzzle, cell =>
        {
            if (gameType == GameType.Play)
            {
                if (!cell.IsGiven)
                {
                    UpdateDigit(currentPuzzle, cell.Row, cell.Column, 0, gameType);
                }
            }
            else
            {
                UpdateDigit(currentPuzzle, cell.Row, cell.Column, 0, gameType);
            }
        });
    }
    public void RemoveCandidates(IPuzzle? currentPuzzle, CandidateType type)
    {
        UpdateSelectedCells(currentPuzzle, cell =>
        {
            if (!cell.IsGiven)
                ClearAllCandidates(currentPuzzle, type, cell.Row, cell.Column);
        });
    }
    public void RemoveColors(IPuzzle? currentPuzzle)
    {
        UpdateSelectedCells(currentPuzzle, cell =>
        {
            cell.CellColors.Clear();
        });
    }
    public void SmartRemovalFromSelected(IPuzzle? currentPuzzle, GameType gameType)
    {
        var action = ResolveRemovalAction(currentPuzzle);

        if (action == RemovalAction.None)
            return;

        switch (action)
        {
            case RemovalAction.Digit:
                RemoveDigits(currentPuzzle, gameType);
                break;

            case RemovalAction.CenterCandidates:
                RemoveCandidates(currentPuzzle, CandidateType.CenterCandidates);
                break;

            case RemovalAction.CornerCandidates:
                RemoveCandidates(currentPuzzle, CandidateType.CornerCandidates);
                break;

            case RemovalAction.Colors:
                RemoveColors(currentPuzzle);
                break;
        }
    }
    #endregion

    #region Private Update Methods

    private RemovalAction ResolveRemovalAction(IPuzzle? currentPuzzle)
    {
        bool hasDigits = false;
        bool hasCorner = false;
        bool hasCenter = false;
        bool hasColors = false;

        if (currentPuzzle == null) return RemovalAction.None;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var cell = currentPuzzle[r, c];

                if (!cell.IsSelected) continue;

                if (cell.Digit != 0 && !cell.IsGiven)
                    hasDigits = true;

                if (cell.CornerCandidates.Any())
                    hasCorner = true;

                if (cell.CenterCandidates.Any())
                    hasCenter = true;

                if (cell.CellColors.Any())
                    hasColors = true;
            }
        }

        if (hasDigits) return RemovalAction.Digit;
        if (hasCenter) return RemovalAction.CenterCandidates;
        if (hasCorner) return RemovalAction.CornerCandidates;
        if (hasColors) return RemovalAction.Colors;

        return RemovalAction.None;
    }
    private void UpdateSelectedCells(IPuzzle? currentPuzzle, Action<Cell> action)
    {
        if (currentPuzzle == null || action == null)
            return;

        currentPuzzle.BeginBatchUpdate();

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var cell = currentPuzzle[r, c];
                if (!cell.IsSelected)
                    continue;

                action(cell);
            }
        }

        SudokuValidationService.ValidateMove(currentPuzzle);

        currentPuzzle.EndBatchUpdate();
    }
    private bool AnyCellsSelected(IPuzzle? currentPuzzle)
    {
        if (currentPuzzle == null) return false;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var cell = currentPuzzle[r, c];
                if (cell == null) continue;

                if (cell.IsSelected) return true;
            }
        }
        return false;
    }
    private void UpdateDigit(IPuzzle? currentPuzzle, int row, int column, int digit, GameType gameType = GameType.Play)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));
        if (digit < 0 || digit > 9) throw new ArgumentOutOfRangeException(nameof(digit), "Digit must be between 0 and 9.");

        if (currentPuzzle == null) return;

        var currentDigit = currentPuzzle[row, column].Digit;

        if (currentDigit == digit)
        {
            currentPuzzle[row, column].Digit = 0;
            if (gameType == GameType.Create) currentPuzzle[row, column].IsGiven = false;
            RestoreCandidatesInAllUnits(currentPuzzle, row, column, currentDigit);
        }
        else
        {
            if (digit == 0)
            {
                currentPuzzle[row, column].Digit = 0;
                if (gameType == GameType.Create) currentPuzzle[row, column].IsGiven = false;
                RestoreCandidatesInAllUnits(currentPuzzle, row, column, currentDigit);
            }
            else
            {
                var oldDigit = currentPuzzle[row, column].Digit;

                currentPuzzle[row, column].Digit = digit;
                if (gameType == GameType.Create) currentPuzzle[row, column].IsGiven = true;

                // Clear all candidates in the current cell
                CandidateManager.ClearAllCandidatesInCell(currentPuzzle, row, column);

                RemoveSolverCandidatesInRelatedUnits(currentPuzzle, row, column, digit);
                RemoveCenterCandidatesInRelatedUnits(currentPuzzle, row, column, digit);
                RemoveCornerCandidatesInRelatedUnits(currentPuzzle, row, column, digit);

                RestoreCandidatesInAllUnits(currentPuzzle, row, column, currentDigit);

            }
        }
    }
    private void RestoreCandidatesInAllUnits(IPuzzle currentPuzzle, int row, int column, int currentDigit)
    {
        CandidateManager.RestoreCandidates(currentPuzzle, CandidateType.CenterCandidates, row, column, currentDigit);
        CandidateManager.RestoreCandidates(currentPuzzle, CandidateType.CornerCandidates, row, column, currentDigit);
        CandidateManager.RestoreCandidates(currentPuzzle, CandidateType.SolverCandidates, row, column, currentDigit);
    }
    private void UpdateCandidate(IPuzzle? currentPuzzle, int row, int column, int digit, CandidateType type)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));
        if (currentPuzzle == null) return;

        switch (type)
        {
            case CandidateType.CenterCandidates:
                if (currentPuzzle[row, column].CenterCandidates.Contains(digit))
                    currentPuzzle[row, column].CenterCandidates[digit] = false;
                else
                    currentPuzzle[row, column].CenterCandidates[digit] = true;
                break;

            case CandidateType.CornerCandidates:
                if (currentPuzzle[row, column].CornerCandidates.Contains(digit))
                    currentPuzzle[row, column].CornerCandidates[digit] = false;
                else
                    currentPuzzle[row, column].CornerCandidates[digit] = true;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Unknown CandidateType");
        }
    }
    private void ClearAllCandidates(IPuzzle? currentPuzzle, CandidateType candidateType, int row, int column)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));
        if (currentPuzzle == null) return;

        switch (candidateType)
        {
            case CandidateType.CenterCandidates:
                currentPuzzle[row, column].CenterCandidates.Clear();
                break;
            case CandidateType.CornerCandidates:
                currentPuzzle[row, column].CornerCandidates.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(candidateType), "Unknown CandidateType");
        }
    }
    private void RemoveSolverCandidatesInRelatedUnits(IPuzzle? currentPuzzle, int row, int column, int digit)
    {
        if (currentPuzzle == null) return;
        int boxRow = row / 3;
        int boxCol = column / 3;
        int boxIndex = boxRow * 3 + boxCol;
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Row, CandidateType.SolverCandidates, row, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Column, CandidateType.SolverCandidates, column, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Box, CandidateType.SolverCandidates, boxIndex, (row, column), digit);
    }
    private void RemoveCenterCandidatesInRelatedUnits(IPuzzle? currentPuzzle, int row, int column, int digit)
    {
        if (currentPuzzle == null) return;
        int boxRow = row / 3;
        int boxCol = column / 3;
        int boxIndex = boxRow * 3 + boxCol;
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Row, CandidateType.CenterCandidates, row, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Column, CandidateType.CenterCandidates, column, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Box, CandidateType.CenterCandidates, boxIndex, (row, column), digit);
    }
    private void RemoveCornerCandidatesInRelatedUnits(IPuzzle? currentPuzzle, int row, int column, int digit)
    {
        if (currentPuzzle == null) return;
        int boxRow = row / 3;
        int boxCol = column / 3;
        int boxIndex = boxRow * 3 + boxCol;
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Row, CandidateType.CornerCandidates, row, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Column, CandidateType.CornerCandidates, column, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(currentPuzzle, UnitType.Box, CandidateType.CornerCandidates, boxIndex, (row, column), digit);
    }
    #endregion
}
