using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Solver.ConstraintSolver.Utils;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class JellyfishConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 3;

    private readonly (int mask, int candidate)[] _rowMasks = new (int mask, int candidate)[Puzzle.Size];
    private readonly (int mask, int candidate)[] _colMasks = new (int mask, int candidate)[Puzzle.Size];

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        var result = false;
        for (int candidate = 1; candidate <= IPuzzle.Size; candidate++)
        {
            result |= FindJellyfish(UnitType.Row, candidate, buffer);
            result |= FindJellyfish(UnitType.Column, candidate, buffer);
        }
        errorMessage = "";
        return result;
    }

    private bool FindJellyfish(UnitType unitType, int candidate, Span<(int row, int col, int digit, int mask)> buffer)
    {
        if (unitType == UnitType.Row)
        {
            Array.Clear(_rowMasks, 0, _rowMasks.Length); // alte Werte löschen
            for (int r = 0; r < Puzzle.Size; r++)
            {
                int m = 0;
                for (int c = 0; c < Puzzle.Size; c++)
                    if (_puzzle[r, c].Digit == 0 && _puzzle[r, c].SolverCandidates.Contains(candidate))
                        m |= (1 << c);

                if (ConstraintHelper.CountCandidate(m) is not > 4)
                    _rowMasks[r] = (m, candidate);
            }
        }
        else if (unitType == UnitType.Column)
        {
            Array.Clear(_colMasks, 0, _colMasks.Length); // alte Werte löschen
            for (int c = 0; c < Puzzle.Size; c++)
            {
                int m = 0;
                for (int r = 0; r < Puzzle.Size; r++)
                    if (_puzzle[r, c].Digit == 0 && _puzzle[r, c].SolverCandidates.Contains(candidate))
                        m |= (1 << r);

                if (ConstraintHelper.CountCandidate(m) is not > 4)
                    _colMasks[c] = (m, candidate);
            }
        }

        int bufferCount = 0;
        bool found = false;

        // 4er-Kombinationen durchgehen
        for (int i0 = 0; i0 < Puzzle.Size - 3; i0++)
            for (int i1 = i0 + 1; i1 < Puzzle.Size - 2; i1++)
                for (int i2 = i1 + 1; i2 < Puzzle.Size - 1; i2++)
                    for (int i3 = i2 + 1; i3 < Puzzle.Size; i3++)
                    {
                        int m0 = unitType == UnitType.Row ? _rowMasks[i0].mask : _colMasks[i0].mask;
                        int m1 = unitType == UnitType.Row ? _rowMasks[i1].mask : _colMasks[i1].mask;
                        int m2 = unitType == UnitType.Row ? _rowMasks[i2].mask : _colMasks[i2].mask;
                        int m3 = unitType == UnitType.Row ? _rowMasks[i3].mask : _colMasks[i3].mask;

                        if (m0 == 0 || m1 == 0 || m2 == 0 || m3 == 0)
                            continue;

                        int combinedMask = m0 | m1 | m2 | m3;

                        if (ConstraintHelper.CountCandidate(combinedMask) == 4)
                        {
                            // gültiger Jellyfish → jetzt Kandidaten entfernen
                            if (EliminateCandidates(unitType, combinedMask, i0, i1, i2, i3, candidate, ref bufferCount, buffer))
                                found = true;
                        }
                    }

        return found;
    }
    private bool EliminateCandidates(UnitType unitType, int mask, int u0, int u1, int u2, int u3, int candidate, ref int bufferCount, Span<(int row, int col, int digit, int mask)> buffer)
    {

        foreach (int i in GetSetBits(mask))
        {
            for (int j = 0; j < Puzzle.Size; j++)
            {
                if (j == u0 || j == u1 || j == u2 || j == u3) continue;


                var cell = unitType switch
                {
                    UnitType.Row => _puzzle[j, i],
                    UnitType.Column => _puzzle[i, j],
                    _ => throw new NotSupportedException("This unit is not supported in a X-Wing!")
                };

                if (cell.Digit != 0) continue;

                if (cell.SolverCandidates.Contains(candidate))
                {
                    // Eintrag in den Buffer schreiben
                    if (bufferCount < buffer.Length)
                    {
                        buffer[bufferCount++] = (cell.Row, cell.Column, candidate, 0);
                    }

                    _puzzle[cell.Row, cell.Column].SolverCandidates.Remove(candidate);
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerable<int> GetSetBits(int mask)
    {
        for (int i = 0; i < Puzzle.Size; i++)
            if ((mask & (1 << i)) != 0)
                yield return i;
    }
}
