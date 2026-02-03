using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

namespace WindowsSudoku2026.Solver.ConstraintSolver;

public class ConstraintSolver : IDisposable, IConstraintSolver
{
    private readonly HashSet<string> _usedConstraints;
    private readonly ConstraintManagerV2 _constraintManager;
    private readonly IPuzzle _puzzle;
    private bool _hasLogicError;
    private int _currentComplexity = 1;

    public event EventHandler<string>? StepApplied;
    public event EventHandler? SolvingFinished;

    public ConstraintSolver(IPuzzle puzzle)
    {
        _usedConstraints = [];
        _constraintManager = new ConstraintManagerV2();
        _puzzle = puzzle;

        InitializeConstraints();
    }

    public HashSet<string> UsedConstraints => _usedConstraints;

    private void InitializeConstraints()
    {

        _constraintManager.AddConstraint(new NakedSingleConstraint(_puzzle));
        _constraintManager.AddConstraint(new HiddenSingleConstraint(_puzzle));
        _constraintManager.AddConstraint(new NakedPairConstraint(_puzzle));
        _constraintManager.AddConstraint(new NakedTripletConstraint(_puzzle));
        _constraintManager.AddConstraint(new NakedQuadConstraint(_puzzle));
        _constraintManager.AddConstraint(new HiddenPairConstraint(_puzzle));
        //_constraintManager.AddConstraint(new HiddenTripletConstraint(_puzzleModel));
        _constraintManager.AddConstraint(new PointingPairConstraint(_puzzle));
        _constraintManager.AddConstraint(new YWingConstraint(_puzzle));
        _constraintManager.AddConstraint(new XWingConstraint(_puzzle));
        //_constraintManager.AddConstraint(new SkyscraperConstraint(_puzzleModel));
        _constraintManager.AddConstraint(new SwordfishConstraint(_puzzle));
        _constraintManager.AddConstraint(new JellyfishConstraint(_puzzle));

        _constraintManager.ConstraintFailed += (s, e) =>
        {
            _hasLogicError = true; // Merken, dass etwas schiefgelaufen ist
        };
    }

    public IPuzzle Puzzle => _puzzle;

    public bool Solve(bool useHeapBuffer = false)
    {

        if (useHeapBuffer)
        {
            // Heap-Array für Hintergrund-/Warmup-Aufrufe (kein stackalloc)
            var heapBuffer = new (int row, int col, int digit, int mask)[IPuzzle.Size * IPuzzle.Size];
            return SolveWithBuffer(heapBuffer.AsSpan());
        }
        else
        {
            // Stackalloc für UI-/Hot-Path (schnell, allocation-free)
            Span<(int row, int col, int digit, int mask)> stackBuffer = stackalloc (int, int, int, int)[IPuzzle.Size * IPuzzle.Size];
            return SolveWithBuffer(stackBuffer);
        }
    }
    private bool SolveWithBuffer(Span<(int row, int col, int digit, int mask)> buffer)
    {
        bool anyConstraints;
        bool overallChange;
        _hasLogicError = false; // Reset

        _usedConstraints.Clear();

        //TODO: erst die einfachen constraints, dann die komplexeren
        do
        {
            overallChange = false;
            // check erst die complexity stufe 1      
            for (int complexity = 1; complexity <= 3; complexity++)
            {
                do
                {
                    if (IsSolved())
                        return true;

                    _constraintManager.ApplyAllConstraints(complexity, out anyConstraints, buffer);

                    if (anyConstraints)
                    {
                        overallChange = true;

                        // --- KORREKTUR: Kompromiss-Check ---
                        // Wir scannen das Board auf Widersprüche (Zelle ohne Digit und ohne Candidates)
                        if (HasGlobalLogicError())
                        {
                            _hasLogicError = true;
                            return false; // Sofortiger Abbruch der gesamten Solver-Schleife
                        }
                    }

                    if (_constraintManager.CurrentConstraint is Constraint c)
                    {
                        //Debug.WriteLine($"Applied Constraint: {c.GetType().Name} at complexity {complexity}");
                        _usedConstraints.Add(c.GetType().Name);
                    }
                } while (anyConstraints);
            }

        } while (overallChange); // TODO refine: Abbruchbedingung anpassen

        return IsSolved() && !_hasLogicError;
    }
    private bool IsSolved()
    {
        for (int row = 0; row < IPuzzle.Size; row++)
        {
            for (int col = 0; col < IPuzzle.Size; col++)
            {
                if (Puzzle[row, col].Digit == 0)
                    return false;
            }
        }
        return true;
    }
    private bool HasGlobalLogicError()
    {
        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                // Nutzt deinen vorhandenen Check im EliminationHelper
                if (Utils.EliminationHelper.IsUnsolvableCell(_puzzle, r, c))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Dispose()
    {
        _constraintManager.Clear();
    }
}
