using System.Numerics;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Solver.ConstraintSolver.Utils;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class NakedSingleConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 1;

    private bool FindNakedSingle()
    {
        for (int row = 0; row < Puzzle.Size; row++)
        {
            for (int col = 0; col < Puzzle.Size; col++)
            {
                if (_puzzle[row, col].Digit != 0) continue;

                // Beispiel: Kandidaten als Bitmaske speichern (z. B. 1..9 für Sudoku)
                // Bit 0 = Kandidat 1, Bit 1 = Kandidat 2, ..., Bit 8 = Kandidat 9
                int mask = _puzzle[row, col].SolverCandidates.BitMask;

                // Prüfen ob genau 1 Bit gesetzt ist:
                if (mask != 0 && (mask & (mask - 1)) == 0)
                {
                    // Bestimme das gesetzte Bit -> entspricht dem Kandidaten
                    int digit = BitOperations.TrailingZeroCount(mask) + 1;
                    // (TrailingZeroCount gibt 0-basiert die Position des einzigen gesetzten Bits zurück)

                    EliminationHelper.SetDigit(_puzzle, row, col, digit);

                    // Kandidaten löschen (Maske auf 0 setzen)
                    _puzzle[row, col].SolverCandidates.Clear();
                    return true;
                }
            }
        }
        return false;
    }

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        if (FindNakedSingle())
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = "No naked single found.";
        return false;
    }
}
