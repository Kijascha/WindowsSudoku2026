using System.Text;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class PointingPairConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 2;
    private readonly HashSet<(int, int, int, int, int)> _seenPointingPairs = new();

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        StringBuilder debugInfo = new StringBuilder();
        bool foundValidPointingPair = false;
        _seenPointingPairs.Clear();

        // Iterate through all candidates (1-9)
        for (int candidate = 1; candidate <= 9; candidate++)
        {
            //debugInfo.AppendLine($"Checking candidate {candidate} for pointing pairs...");
            foundValidPointingPair |= CheckForPointingPairInBlock(UnitType.Column, candidate, buffer, debugInfo);
            foundValidPointingPair |= CheckForPointingPairInBlock(UnitType.Row, candidate, buffer, debugInfo);
        }

        errorMessage = foundValidPointingPair ? "" : "No valid pointing pairs found.";

        //Debug.WriteLine(debugInfo.ToString());

        return foundValidPointingPair;
    }

    private bool CheckForPointingPairInBlock(UnitType unitType, int candidate, Span<(int row, int col, int digit, int mask)> buffer, StringBuilder debugInfo)
    {
        bool foundPointingPair = false;

        for (int unit = 0; unit < Puzzle.Size; unit++)
        {
            var candidateCount = CountOccurrencesInUnit(unitType, unit, candidate, buffer);
            if (candidateCount == 2)
                foundPointingPair |= FindPointingPairInUnit(unitType, unit, candidate, buffer, debugInfo);
        }

        return foundPointingPair;
    }

    private bool FindPointingPairInUnit(UnitType unitType, int startUnit, int candidate, Span<(int row, int col, int digit, int mask)> buffer, StringBuilder debugInfo)
    {
        ReadOnlySpan<Cell> unit = unitType switch
        {
            UnitType.Row => _puzzle.GetRowSpan(startUnit),
            UnitType.Column => _puzzle.GetColumnSpan(startUnit),
            UnitType.Box => _puzzle.GetBoxSpan(startUnit),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType))
        };

        var filteredUnit = Filter(unit, candidate, buffer);

        if (filteredUnit.Length == 2)
        {
            var possiblePointingCellA = filteredUnit[0];
            var possiblePointingCellB = filteredUnit[1];

            var a = filteredUnit[0];
            var b = filteredUnit[1];

            // Prüfen ob sie in derselben Box liegen – das ist die entscheidende Bedingung!
            bool sameBox = (a.row / 3 == b.row / 3) && (a.col / 3 == b.col / 3);
            if (!sameBox)
                return false;

            var pointingPair = (a.row, a.col, b.row, b.col, candidate);

            _seenPointingPairs.Add(pointingPair);

            var box = _puzzle.GetBoxSpan(a.row / 3, a.col / 3);
            var nonPairCellsInBox = Where(box, candidate, a, b, buffer);

            if (nonPairCellsInBox.Length == 0)
                return false;

            foreach (var cell in nonPairCellsInBox)
                _puzzle[cell.row, cell.col].SolverCandidates.Remove(candidate);

            return true;
        }

        return false;
    }
    private static ReadOnlySpan<(int row, int col, int digit, int mask)> Where(ReadOnlySpan<Cell> unit, int candidate, (int row, int col, int digit, int mask) possiblePointingCellA, (int row, int col, int digit, int mask) possiblePointingCellB, Span<(int row, int col, int digit, int mask)> buffer)
    {
        int count = 0;

        for (int i = 0; i < unit.Length; i++)
        {
            if (!(unit[i].Row == possiblePointingCellA.row && unit[i].Column == possiblePointingCellA.col) &&
                !(unit[i].Row == possiblePointingCellB.row && unit[i].Column == possiblePointingCellB.col) &&
                unit[i].SolverCandidates.Contains(candidate))
            {
                buffer[count++] = (unit[i].Row, unit[i].Column, unit[i].Digit, unit[i].SolverCandidates.BitMask);
            }
        }
        return buffer.Slice(0, count);
    }
    private static ReadOnlySpan<(int row, int col, int digit, int mask)> Filter(ReadOnlySpan<Cell> unit, int candidate, Span<(int row, int col, int digit, int mask)> buffer)
    {
        int count = 0;

        for (int i = 0; i < unit.Length; i++)
        {
            if (unit[i].Digit == 0 && unit[i].SolverCandidates.Contains(candidate))
            {
                buffer[count++] = (unit[i].Row, unit[i].Column, unit[i].Digit, unit[i].SolverCandidates.BitMask);
            }
        }
        return buffer.Slice(0, count);
    }
    public static bool InSameUnit(UnitType unitType, int row1, int col1, int row2, int col2)
    {
        return unitType switch
        {
            UnitType.Row => row1 == row2,
            UnitType.Column => col1 == col2,
            UnitType.Box => (row1 / 3 == row2 / 3 && col1 / 3 == col2 / 3),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), "Invalid unit type")
        };
    }
    public int CountOccurrencesInUnit(UnitType unitType, int unitIndex, int candidate, Span<(int row, int col, int digit, int mask)> buffer)
    {
        int count = 0;

        var unit = unitType switch
        {
            UnitType.Row => Filter(_puzzle.GetRowSpan(unitIndex), candidate, buffer),
            UnitType.Column => Filter(_puzzle.GetColumnSpan(unitIndex), candidate, buffer),
            UnitType.Box => Filter(_puzzle.GetBoxSpan(unitIndex), candidate, buffer),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), "Invalid unit type")
        };

        count = unit.Length;
        return count;
    }
}
