using System.Numerics;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class NakedTripletConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 1;

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        errorMessage = "";

        if (!FindNakedTriplet(buffer))
        {
            errorMessage = "Couldn't find any Naked Triplets!";
            return false;
        }
        return true;
    }

    private bool FindNakedTriplet(Span<(int row, int col, int digit, int mask)> buffer)
    {
        // Wir iterieren über alle Units: 9 Zeilen, 9 Spalten, 9 Boxen
        for (int i = 0; i < Puzzle.Size; i++)
        {
            if (FindNakedTripletInUnit(UnitType.Row, i, buffer))
                return true;
            if (FindNakedTripletInUnit(UnitType.Column, i, buffer))
                return true;
            if (FindNakedTripletInUnit(UnitType.Box, i, buffer))
                return true;
        }
        return false;
    }
    private bool FindNakedTripletInUnit(UnitType unitType, int unitIndex, Span<(int row, int col, int digit, int mask)> buffer)
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
            int popCount = BitOperations.PopCount((uint)cell.SolverCandidates.BitMask);
            if (cell.Digit == 0 && popCount >= 2 && popCount <= 3)
                buffer[candidateCount++] = (cell.Row, cell.Column, cell.Digit, cell.SolverCandidates.BitMask);
        }

        bool changed = false;

        // Vergleiche alle Triplets
        for (int i = 0; i < candidateCount - 2; i++)
        {
            for (int j = i + 1; j < candidateCount - 1; j++)
            {
                for (int k = j + 1; k < candidateCount; k++)
                {
                    // Kandidatenmengen vereinigen
                    int combinedMask = buffer[i].mask | buffer[j].mask | buffer[k].mask;

                    // Nur weitermachen, wenn die Vereinigung genau 3 Kandidaten enthält
                    if (BitOperations.PopCount((uint)combinedMask) != 3)
                        continue;

                    int idxARow = buffer[i].row;
                    int idxACol = buffer[i].col;
                    int idxBRow = buffer[j].row;
                    int idxBCol = buffer[j].col;
                    int idxCRow = buffer[k].row;
                    int idxCCol = buffer[k].col;

                    //Debug.WriteLine($"Naked Triplet found {unitType} - Mask {Convert.ToString(combinedMask, 2).PadLeft(9, '0')}: " +
                    //                $"({idxARow},{idxACol}) & ({idxBRow},{idxBCol}) & ({idxCRow},{idxCCol})");

                    // Entferne Kandidaten aus allen anderen Zellen im Unit
                    for (int l = 0; l < unit.Length; l++)
                    {
                        (int row, int col) = (unit[l].Row, unit[l].Column);

                        // Überspringe die beiden Triplet-Zellen
                        if ((row == idxARow && col == idxACol) || (row == idxBRow && col == idxBCol) || (row == idxCRow && col == idxCCol))
                            continue;

                        int beforeMask = _puzzle[row, col].SolverCandidates.BitMask;

                        // Skip Zellen mit exakt der gleichen Mask
                        if (beforeMask == combinedMask) continue;

                        int newMask = beforeMask & ~combinedMask;
                        if (newMask != beforeMask)
                        {
                            _puzzle[row, col].SolverCandidates.BitMask = newMask;
                            //Debug.WriteLine($"  Candidates updated at ({row},{col}): {Convert.ToString(beforeMask, 2).PadLeft(9, '0')} -> {Convert.ToString(newMask, 2).PadLeft(9, '0')}");
                            changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }
}
