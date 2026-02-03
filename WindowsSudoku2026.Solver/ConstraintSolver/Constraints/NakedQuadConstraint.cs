using System.Numerics;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class NakedQuadConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 1;

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        errorMessage = "";

        if (!FindNakedQuad(buffer))
        {
            errorMessage = "Couldn't find any Naked Quads!";
            return false;
        }
        return true;
    }

    private bool FindNakedQuad(Span<(int row, int col, int digit, int mask)> buffer)
    {
        // Wir iterieren über alle Units: 9 Zeilen, 9 Spalten, 9 Boxen
        for (int i = 0; i < Puzzle.Size; i++)
        {
            if (FindNakedQuadInUnit(UnitType.Row, i, buffer))
                return true;
            if (FindNakedQuadInUnit(UnitType.Column, i, buffer))
                return true;
            if (FindNakedQuadInUnit(UnitType.Box, i, buffer))
                return true;
        }
        return false;
    }
    private bool FindNakedQuadInUnit(UnitType unitType, int unitIndex, Span<(int row, int col, int digit, int mask)> buffer)
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
            if (cell.Digit == 0 && popCount >= 2 && popCount <= 4)
                buffer[candidateCount++] = (cell.Row, cell.Column, cell.Digit, cell.SolverCandidates.BitMask);
        }

        bool changed = false;

        // Vergleiche alle Quads
        for (int i = 0; i < candidateCount - 3; i++)
        {
            for (int j = i + 1; j < candidateCount - 2; j++)
            {
                for (int k = j + 1; k < candidateCount - 1; k++)
                {
                    for (int l = k + 1; l < candidateCount; l++)
                    {
                        // Kandidatenmengen vereinigen
                        int combinedMask = buffer[i].mask | buffer[j].mask | buffer[k].mask | buffer[l].mask;

                        // Nur weitermachen, wenn die Vereinigung genau 3 Kandidaten enthält
                        if (BitOperations.PopCount((uint)combinedMask) != 4)
                            continue;

                        int idxARow = buffer[i].row;
                        int idxACol = buffer[i].col;
                        int idxBRow = buffer[j].row;
                        int idxBCol = buffer[j].col;
                        int idxCRow = buffer[k].row;
                        int idxCCol = buffer[k].col;
                        int idxDRow = buffer[l].row;
                        int idxDCol = buffer[l].col;

                        //Debug.WriteLine($"Naked Quad found {unitType} - Mask {Convert.ToString(combinedMask, 2).PadLeft(9, '0')}: " +
                        //                $"({idxARow},{idxACol}) & ({idxBRow},{idxBCol}) & ({idxCRow},{idxCCol}) & ({idxDRow},{idxDCol})");

                        // Entferne Kandidaten aus allen anderen Zellen im Unit
                        for (int m = 0; m < unit.Length; m++)
                        {
                            (int row, int col) = (unit[m].Row, unit[m].Column);

                            // Überspringe die beiden Quad-Zellen
                            if ((row == idxARow && col == idxACol) ||
                                (row == idxBRow && col == idxBCol) ||
                                (row == idxCRow && col == idxCCol) ||
                                (row == idxDRow && col == idxDCol))
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
        }

        return changed;
    }
}
