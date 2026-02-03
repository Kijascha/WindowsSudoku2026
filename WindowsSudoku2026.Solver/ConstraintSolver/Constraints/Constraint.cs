using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal abstract class Constraint(IPuzzle puzzle)
{
    protected IPuzzle _puzzle = puzzle;
    public virtual int Complexity { get; init; } = 0;
    public abstract bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer);
}
