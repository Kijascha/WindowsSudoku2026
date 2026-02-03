
using WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

namespace WindowsSudoku2026.Solver.ConstraintSolver;

internal class ConstraintErrorEventArgs(Constraint constraint, string errorMessage) : EventArgs
{
    public Constraint Constraint { get; } = constraint;
    public string ErrorMessage { get; } = errorMessage;
}

