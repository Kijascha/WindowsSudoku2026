using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver
{
    public interface IConstraintSolver
    {
        IPuzzle Puzzle { get; }
        HashSet<string> UsedConstraints { get; }

        event EventHandler? SolvingFinished;
        event EventHandler<string>? StepApplied;

        void Dispose();
        bool Solve(bool useHeapBuffer = false);
    }
}