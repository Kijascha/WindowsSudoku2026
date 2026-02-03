using System.Numerics;
using System.Text;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class YWingConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 2;
    private readonly HashSet<string> _seenYWings = [];

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        bool removedSuccessfully = false;
        StringBuilder debugInfo = new StringBuilder();

        debugInfo.AppendLine("Starting Y-Wing constraint application.");

        // Iterate through all cells to look for potential hinge cells (A).
        for (int row = 0; row < Puzzle.Size; row++)
        {
            for (int col = 0; col < Puzzle.Size; col++)
            {
                if (_puzzle[row, col].Digit != 0) continue;

                var cellCandidates = _puzzle[row, col].SolverCandidates.BitMask;

                debugInfo.AppendLine($"Checking cell ({row}, {col}) with digit {_puzzle[row, col].Digit} and candidates: [{Convert.ToString(cellCandidates, 2).PadLeft(9, '0')}]");

                // A hinge cell must have exactly 2 candidates.
                if (BitOperations.PopCount((uint)cellCandidates) == 2)
                {
                    int hingeCandidates = cellCandidates;
                    debugInfo.AppendLine($"Found hinge cell at ({row}, {col}) with candidates: [{Convert.ToString(hingeCandidates, 2).PadLeft(9, '0')}]");

                    // Look for potential wings in rows, columns, and blocks.
                    removedSuccessfully |= FindYWing(row, col, hingeCandidates, debugInfo);
                }
            }
        }

        errorMessage = removedSuccessfully ? "" : "Couldn't find any Y-Wings";

        // Output the debug information if any Y-Wing patterns were found.
        if (debugInfo.Length > 0)
        {
            //Debug.WriteLine(debugInfo.ToString());
        }

        return removedSuccessfully;
    }
    private bool FindYWing(int hingeRow, int hingeCol, int hingeCandidates, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;

        debugInfo.AppendLine($"Searching for wings for hinge cell at ({hingeRow}, {hingeCol})");

        // Search for wing cells in the column, row, and box.
        removedSuccessfully |= SearchPotentialWings(hingeRow, hingeCol, hingeCandidates, UnitType.Column, debugInfo);
        removedSuccessfully |= SearchPotentialWings(hingeRow, hingeCol, hingeCandidates, UnitType.Row, debugInfo);
        removedSuccessfully |= SearchPotentialWings(hingeRow, hingeCol, hingeCandidates, UnitType.Box, debugInfo);

        return removedSuccessfully;
    }
    private bool SearchPotentialWings(int hingeRow, int hingeCol, int hingeCandidates, UnitType unitType, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;
        ReadOnlySpan<Cell> unit = unitType switch
        {
            UnitType.Row => _puzzle.GetRowSpan(hingeRow),
            UnitType.Column => _puzzle.GetColumnSpan(hingeCol),
            UnitType.Box => _puzzle.GetBoxSpan(hingeRow / 3, hingeCol / 3),
            _ => throw new ArgumentOutOfRangeException(nameof(unitType))
        };


        foreach (var potentialWingCell in unit)
        {
            if (potentialWingCell.SolverCandidates.Count == 2)
            {
                if (potentialWingCell.Digit != 0) continue;

                var potentialWingCandidates = potentialWingCell.SolverCandidates.BitMask;

                if (BitOperations.PopCount((uint)potentialWingCandidates) == 2 && SharesOneCandidate(hingeCandidates, potentialWingCandidates))
                {
                    removedSuccessfully |= TryFindYWingPartner(hingeRow, hingeCol,
                                                               potentialWingCell.Row, potentialWingCell.Column,
                                                               hingeCandidates, potentialWingCandidates, debugInfo);
                }
            }
        }

        return removedSuccessfully;
    }
    private bool TryFindYWingPartner(int hingeRow, int hingeCol, int wing1Row, int wing1Col, int hingeCandidates, int wing1Candidates, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;

        // Search in other units depending on the current search context.
        foreach (var unitType in Enum.GetValues(typeof(UnitType)).Cast<UnitType>())
        {
            ReadOnlySpan<Cell> unit = unitType switch
            {
                UnitType.Row => _puzzle.GetRowSpan(hingeRow),
                UnitType.Column => _puzzle.GetColumnSpan(hingeCol),
                UnitType.Box => _puzzle.GetBoxSpan(hingeRow / 3, hingeCol / 3),
                _ => throw new ArgumentOutOfRangeException(nameof(unitType))
            };

            foreach (var potentialWingCell in unit)
            {
                var potentialWing2Candidates = potentialWingCell.SolverCandidates.BitMask;

                if (BitOperations.PopCount((uint)potentialWing2Candidates) == 2 &&
                    SharesOneCandidate(hingeCandidates, potentialWing2Candidates) &&
                    SharesOneCandidate(wing1Candidates, potentialWing2Candidates))
                {
                    int totalCandidates = hingeCandidates | wing1Candidates | potentialWing2Candidates;

                    if (BitOperations.PopCount((uint)totalCandidates) == 3 &&
                        CandidatesAppearTwice(totalCandidates, hingeCandidates, wing1Candidates, potentialWing2Candidates) &&
                        !InSameUnit(wing1Row, wing1Col, potentialWingCell.Row, potentialWingCell.Column))
                    {
                        int sharedCandidate = GetSharedCandidate(wing1Candidates, potentialWing2Candidates);

                        var yWingPattern = NormalizePattern((hingeRow, hingeCol), (wing1Row, wing1Col), (potentialWingCell.Row, potentialWingCell.Column));

                        if (_seenYWings.Contains(yWingPattern))
                        {
                            continue;
                        }

                        _seenYWings.Add(yWingPattern);

                        removedSuccessfully |= RemoveFromAffectedCells(hingeRow, hingeCol, wing1Row, wing1Col, potentialWingCell.Row, potentialWingCell.Column, sharedCandidate, debugInfo);
                    }
                }
            }
        }
        return removedSuccessfully;
    }
    private bool RemoveFromAffectedCells(int hingeRow, int hingeCol, int wing1Row, int wing1Col, int wing2Row, int wing2Col, int sharedCandidate, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;

        // Go through all cells to find ones that can see both wings.
        for (int row = 0; row < Puzzle.Size; row++)
        {
            for (int col = 0; col < Puzzle.Size; col++)
            {
                // The affected cell cannot be the hinge cell or one of the wing cells.
                if ((row == hingeRow && col == hingeCol) ||
                    (row == wing1Row && col == wing1Col) ||
                    (row == wing2Row && col == wing2Col))
                {
                    continue;
                }

                if (CanSeeBothWings(row, col, wing1Row, wing1Col, wing2Row, wing2Col) && _puzzle[row, col].SolverCandidates.Contains(sharedCandidate))
                {
                    _puzzle[row, col].SolverCandidates.Remove(sharedCandidate);
                    removedSuccessfully = true;
                }
            }
        }

        return removedSuccessfully;
    }
    private bool CandidatesAppearTwice(int totalMask, int hingeMask, int wing1Mask, int wing2Mask)
    {
        int combined = totalMask;

        // Iteriere nur über gesetzte Bits (Kandidaten, die überhaupt vorkommen)
        while (combined != 0)
        {
            int bit = combined & -combined;  // isoliert das niedrigst gesetzte Bit
            combined &= ~bit;                // entfernt das Bit aus combined

            int count = 0;
            if ((hingeMask & bit) != 0) count++;
            if ((wing1Mask & bit) != 0) count++;
            if ((wing2Mask & bit) != 0) count++;

            if (count != 2)
                return false; // Kandidat kommt nicht genau 2x vor
        }

        return true; // alle Kandidaten kommen genau 2x vor
    }
    private bool CanSeeBothWings(int row, int col, int wing1Row, int wing1Col, int wing2Row, int wing2Col)
    {
        // A cell can see both wings if it's in the same row, column, or block as both wing cells.
        return (InSameUnit(row, col, wing1Row, wing1Col) && InSameUnit(row, col, wing2Row, wing2Col));
    }
    private static bool InSameUnit(int r1, int c1, int r2, int c2)
    {
        return r1 == r2 || c1 == c2 || (r1 / 3 == r2 / 3 && c1 / 3 == c2 / 3);
    }
    private bool SharesOneCandidate(int mask1, int mask2)
    {
        int shared = mask1 & mask2; // nur die gemeinsamen Kandidaten
        return BitOperations.PopCount((uint)shared) == 1;
    }
    private int GetSharedCandidate(int mask1, int mask2)
    {
        int shared = mask1 & mask2; // gemeinsames Bit
        if (BitOperations.PopCount((uint)shared) != 1)
            return 0; // 0 = kein eindeutiger gemeinsamer Kandidat

        // Bitposition (0-basiert) → Kandidat (1-basiert)
        return BitOperations.TrailingZeroCount((uint)shared) + 1;
    }
    private bool IsNotTheSameCell(int cell1Row, int cell1Col, int cell2Row, int cell2Col)
    {
        return !(cell1Row == cell2Row && cell1Col == cell2Col);
    }
    private string NormalizePattern((int, int) hinge, (int, int) wing1, (int, int) wing2)
    {
        var cells = new[] { hinge, wing1, wing2 };
        Array.Sort(cells);
        return string.Join("-", cells.Select(c => $"{c.Item1},{c.Item2}"));
    }

}
