using System.Numerics;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class NakedPairConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 1;

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        errorMessage = string.Empty;

        if (!FindNakedPair(buffer))
        {
            errorMessage = "Couldn't find any Naked Pairs!";
            return false;
        }
        return true;
    }

    private bool FindNakedPair(Span<(int row, int col, int digit, int mask)> buffer)
    {
        // Wir iterieren über alle Units: 9 Zeilen, 9 Spalten, 9 Boxen
        for (int i = 0; i < Puzzle.Size; i++)
        {
            if (FindNakedPairInUnit(UnitType.Row, i, buffer)) return true;
            if (FindNakedPairInUnit(UnitType.Column, i, buffer)) return true;
            if (FindNakedPairInUnit(UnitType.Box, i, buffer)) return true;
        }
        return false;
    }

    /// <summary>
    /// Findet und verarbeitet Naked Pairs innerhalb eines Units (Row, Column oder Box).
    /// </summary>
    private bool FindNakedPairInUnit(UnitType unitType, int unitIndex, Span<(int row, int col, int digit, int mask)> buffer)
    {
        ReadOnlySpan<Cell> unit = unitType switch
        {
            UnitType.Row => _puzzle.GetRowSpan(unitIndex),
            UnitType.Column => _puzzle.GetColumnSpan(unitIndex),
            UnitType.Box => _puzzle.GetBoxSpan(unitIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType))
        };

        int candidateCount = 0;
        for (int i = 0; i < unit.Length; i++)
        {
            var cell = unit[i];
            if (cell.Digit == 0 && BitOperations.PopCount((uint)cell.SolverCandidates.BitMask) == 2)
            {
                buffer[candidateCount++] = (cell.Row, cell.Column, cell.Digit, cell.SolverCandidates.BitMask);
            }
        }

        bool changed = false;

        // Vergleiche alle Paare
        for (int i = 0; i < candidateCount - 1; i++)
        {
            for (int j = i + 1; j < candidateCount; j++)
            {
                if (buffer[i].mask != buffer[j].mask)
                    continue;

                int mask = buffer[i].mask;
                int idxARow = buffer[i].row;
                int idxACol = buffer[i].col;
                int idxBRow = buffer[j].row;
                int idxBCol = buffer[j].col;

                //Debug.WriteLine($"Naked Pair found {unitType} - Mask {Convert.ToString(mask, 2).PadLeft(9, '0')}: " +
                //                $"({idxARow},{idxACol}) & ({idxBRow},{idxBCol})");

                // Entferne Kandidaten aus allen anderen Zellen im Unit
                for (int k = 0; k < unit.Length; k++)
                {
                    (int row, int col) = (unit[k].Row, unit[k].Column);

                    // Überspringe die beiden Pair-Zellen
                    if ((row == idxARow && col == idxACol) || (row == idxBRow && col == idxBCol))
                        continue;

                    int beforeMask = _puzzle[row, col].SolverCandidates.BitMask;

                    // Skip Zellen mit exakt der gleichen Mask
                    if (beforeMask == mask) continue;

                    int newMask = beforeMask & ~mask;
                    if (newMask != beforeMask)
                    {
                        _puzzle[row, col].SolverCandidates.BitMask = newMask;
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }
}


