using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class HiddenPairConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 1;

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        errorMessage = "";

        if (!FindHiddenPair(buffer))
        {
            errorMessage = "Couldn't find any Hidden Pairs!";
            return false;
        }
        return true;
    }
    private bool FindHiddenPair(Span<(int row, int col, int digit, int mask)> buffer)
    {
        for (int i = 0; i < Puzzle.Size; i++)
        {
            if (FindHiddenPairInUnit(UnitType.Row, i, buffer)) return true;
            if (FindHiddenPairInUnit(UnitType.Column, i, buffer)) return true;
            if (FindHiddenPairInUnit(UnitType.Box, i, buffer)) return true;
        }
        return false;
    }

    private bool FindHiddenPairInUnit(UnitType unitType, int unitIndex, Span<(int row, int col, int digit, int mask)> buffer)
    {
        ReadOnlySpan<Cell> unit = unitType switch
        {
            UnitType.Row => _puzzle.GetRowSpan(unitIndex),
            UnitType.Column => _puzzle.GetColumnSpan(unitIndex),
            UnitType.Box => _puzzle.GetBoxSpan(unitIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType))
        };

        int cellCount = 0;
        for (int i = 0; i < unit.Length; i++)
        {
            var cell = unit[i];
            if (cell.Digit == 0)
            {
                buffer[cellCount++] = (cell.Row, cell.Column, cell.Digit, cell.SolverCandidates.BitMask);
            }
        }

        bool changed = false;

        for (int candidate1 = 1; candidate1 <= Puzzle.Size - 1; candidate1++)
        {
            int candidate1Mask = 1 << (candidate1 - 1);

            for (int candidate2 = candidate1 + 1; candidate2 <= Puzzle.Size; candidate2++)
            {
                int candidate2Mask = 1 << (candidate2 - 1);

                //Debug.WriteLine($"Checking candidate pair ({candidate1},{candidate2}) in {unitType} {unitIndex}");

                int foundCount = 0;
                int[] pairIndices = new int[2];

                for (int i = 0; i < cellCount; i++)
                {
                    int mask = buffer[i].mask;
                    if ((mask & candidate1Mask) != 0 && (mask & candidate2Mask) != 0)
                    {
                        if (foundCount < 2)
                            pairIndices[foundCount] = i;
                        foundCount++;
                    }
                }

                if (foundCount == 2)
                {
                    // Hidden Pair gefunden
                    int pairMask = candidate1Mask | candidate2Mask;

                    // Prüfen, dass diese beiden Kandidaten nicht in anderen Zellen vorkommen
                    bool validPair = true;
                    for (int i = 0; i < cellCount; i++)
                    {
                        if (i == pairIndices[0] || i == pairIndices[1]) continue;

                        int mask = buffer[i].mask;
                        if ((mask & pairMask) != 0)
                        {
                            validPair = false;
                            break;
                        }
                    }

                    if (!validPair)
                    {
                        //Debug.WriteLine($"Candidate pair ({candidate1},{candidate2}) is not a valid hidden pair (occurs elsewhere).");
                        continue; // kein echtes Hidden Pair
                    }

                    // Hidden Pair gefunden
                    //Debug.WriteLine($"Hidden Pair ({candidate1},{candidate2}) found at cells ({buffer[pairIndices[0]].row},{buffer[pairIndices[0]].col}) and ({buffer[pairIndices[1]].row},{buffer[pairIndices[1]].col})");

                    // Nur die beiden Zellen anpassen
                    for (int j = 0; j < 2; j++)
                    {
                        int idx = pairIndices[j];
                        int newMask = buffer[idx].mask & pairMask; // nur Kandidaten des Pairs behalten
                        if (newMask != buffer[idx].mask)
                        {
                            //Debug.WriteLine($"Updating cell ({buffer[idx].row},{buffer[idx].col}) mask: {Convert.ToString(buffer[idx].mask, 2).PadLeft(9, '0')} -> {Convert.ToString(newMask, 2).PadLeft(9, '0')}");
                            _puzzle[buffer[idx].row, buffer[idx].col].SolverCandidates.BitMask = newMask;
                            buffer[idx].mask = newMask; // wichtig, damit der buffer aktuell bleibt
                            changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }
}
