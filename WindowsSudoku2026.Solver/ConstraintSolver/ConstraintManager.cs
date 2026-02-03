using WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

namespace WindowsSudoku2026.Solver.ConstraintSolver;

/// <summary>
/// Manages a collection of constraints and applies them iteratively.
/// </summary>
internal class ConstraintManagerV2 : IDisposable
{
    // Deterministische, geordnete Liste der Constraints
    private readonly List<Constraint> _constraintList;

    /// <summary>
    /// Initializes a new instance of the ConstraintManager class.
    /// </summary>
    public ConstraintManagerV2()
    {
        _constraintList = new List<Constraint>();
    }

    public Constraint? CurrentConstraint { get; private set; } = null!;

    public IReadOnlyCollection<Constraint> Constraints => _constraintList.AsReadOnly();

    /// <summary>
    /// Event raised when a constraint fails during application.
    /// </summary>
    public event EventHandler<ConstraintErrorEventArgs>? ConstraintFailed;

    /// <summary>
    /// Adds a constraint to the ConstraintManager.
    /// </summary>
    /// <param name="constraint">The constraint to add.</param>
    /// <returns>True if the constraint was successfully added; otherwise, false.</returns>
    public bool AddConstraint(Constraint constraint)
    {
        if (constraint == null) return false;
        if (_constraintList.Contains(constraint)) return false;
        _constraintList.Add(constraint);
        return true;
    }

    /// <summary>
    /// Removes a constraint from the ConstraintManager.
    /// </summary>
    /// <param name="constraint">The constraint to remove.</param>
    /// <returns>True if the constraint was successfully removed; otherwise, false.</returns>
    public bool RemoveConstraint(Constraint constraint)
    {
        return _constraintList.Remove(constraint);
    }

    /// <summary>
    /// Checks if a constraint is already present in the ConstraintManager.
    /// </summary>
    /// <param name="constraint">The constraint to check.</param>
    /// <returns>True if the constraint is present; otherwise, false.</returns>
    public bool ContainsConstraint(Constraint constraint)
    {
        return _constraintList.Contains(constraint);
    }

    /// <summary>
    /// Applies all constraints stored in the ConstraintManager.
    /// anyConstraintApplied ist true, wenn mindestens ein Constraint eine �nderung durchgef�hrt hat.
    /// False bedeutet nicht automatisch einen Fehler; ein Constraint liefert false wenn er nichts ver�ndert hat.
    /// </summary>
    public bool ApplyAllConstraints(int complexity, out bool anyConstraintApplied, Span<(int row, int col, int digit, int mask)> buffer)
    {
        anyConstraintApplied = false;
        CurrentConstraint = null;

        foreach (var constraint in _constraintList)
        {
            if (constraint.Complexity != complexity) continue;

            try
            {
                var applied = constraint.ApplyConstraint(out string? errorMessage, buffer);

                if (applied)
                {
                    anyConstraintApplied = true;
                    CurrentConstraint = constraint;
                }

                // Wenn die Constraint-Implementierung eine nicht-leere errorMessage liefert,
                // wird diese als Event ausgegeben. So sind Fehler / wichtige Infos sichtbar.
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OnConstraintFailed(new ConstraintErrorEventArgs(constraint, errorMessage));
                }
            }
            catch (Exception ex)
            {
                // Ein echtes Exception-Fall: Event feuern und weiter (oder rethrow, je nach gew�nschtem Verhalten)
                OnConstraintFailed(new ConstraintErrorEventArgs(constraint, ex.Message));
            }
        }

        // R�ckgabewert: true wenn die Durchl�ufe ohne Exceptions passiert sind (hier immer true),
        // das hat in deinem bisherigen Code kaum Verwendung; wichtig ist anyConstraintApplied.
        return true;
    }
    public void Clear()
    {
        _constraintList.Clear();
    }

    /// <summary>
    /// Raises the <see cref="ConstraintFailed"/> event.
    /// </summary>
    /// <param name="e">Event arguments containing details about the failed constraint.</param>
    protected virtual void OnConstraintFailed(ConstraintErrorEventArgs e)
    {
        ConstraintFailed?.Invoke(this, e);
    }

    public void Dispose()
    {
        _constraintList.Clear();
    }
}
