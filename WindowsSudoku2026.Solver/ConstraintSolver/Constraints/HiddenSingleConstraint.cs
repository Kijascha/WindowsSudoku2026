using System.Diagnostics;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class HiddenSingleConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 1;

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        bool debug = false;
        errorMessage = "";
        if (FindHiddenSingles(UnitType.Row, buffer, debug) || FindHiddenSingles(UnitType.Column, buffer, debug) || FindHiddenSingles(UnitType.Box, buffer, debug))
        {
            return true;
        }
        errorMessage = "Couldn't find any Hidden Singles!";
        return false;
    }
    public bool FindHiddenSingles(UnitType unitType, Span<(int row, int col, int digit, int mask)> buffer, bool debug = false)
    {
        for (int row = 0; row < Puzzle.Size; row++)
        {
            for (int col = 0; col < Puzzle.Size; col++)
            {
                // Hole das Unit
                ReadOnlySpan<Cell> unit = unitType switch
                {
                    UnitType.Row => _puzzle.GetRowSpan(row),
                    UnitType.Column => _puzzle.GetColumnSpan(col),
                    UnitType.Box => _puzzle.GetBoxSpan((row / 3) * 3 + (col / 3)),
                    _ => throw new ArgumentOutOfRangeException(nameof(unitType))
                };

                int emptyCellsCount = 0;

                // Fülle filteredUnit mit leeren Zellen
                for (int i = 0; i < unit.Length; i++)
                {
                    var cell = unit[i];
                    if (cell.Digit != 0) continue;

                    buffer[emptyCellsCount++] = (cell.Row, cell.Column, cell.Digit, cell.SolverCandidates.BitMask);
                }

                // Für jeden Kandidaten prüfen, ob er nur einmal vorkommt
                for (int candidate = 1; candidate <= Puzzle.Size; candidate++)
                {
                    int count = 0;
                    int singleIndex = -1;

                    for (int i = 0; i < emptyCellsCount; i++)
                    {
                        // Prüfen, ob Kandidat in der Maske enthalten ist
                        if ((buffer[i].mask & (1 << (candidate - 1))) != 0)
                        {
                            count++;
                            if (count > 1) break; // Early exit
                            singleIndex = i;
                        }
                    }

                    if (count == 1)
                    {
                        // Hidden Single gefunden
                        var singleCell = _puzzle[buffer[singleIndex].row, buffer[singleIndex].col];

                        // Optional: Debug-Ausgabe
                        if (debug)
                        {
                            Debug.WriteLine($"Hidden Single found: Candidate {candidate} at ({singleCell.Row},{singleCell.Column})");
                        }

                        // Transform hidden single -> naked single
                        singleCell.SolverCandidates.Clear();
                        singleCell.SolverCandidates.Add(candidate);

                        return true;
                    }
                }
            }
        }

        return false;
    }
}
