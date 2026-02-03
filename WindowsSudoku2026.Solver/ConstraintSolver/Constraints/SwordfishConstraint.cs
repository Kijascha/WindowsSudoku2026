using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Solver.ConstraintSolver.Utils;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Constraints;

internal class SwordfishConstraint(IPuzzle puzzle) : Constraint(puzzle)
{
    public override int Complexity { get; init; } = 3;

    private readonly (int mask, int candidate)[] _rowMasks = new (int mask, int candidate)[Puzzle.Size];
    private readonly (int mask, int candidate)[] _colMasks = new (int mask, int candidate)[Puzzle.Size];

    public override bool ApplyConstraint(out string errorMessage, Span<(int row, int col, int digit, int mask)> buffer)
    {
        var result = false;
        for (int candidate = 1; candidate <= IPuzzle.Size; candidate++)
        {
            result |= FindSwordfish(UnitType.Row, candidate, buffer);
            result |= FindSwordfish(UnitType.Column, candidate, buffer);
        }
        errorMessage = "";
        return result;
    }

    // for debugging purposes
    //static string MaskToBin(int mask, int size)
    //{
    //    var s = Convert.ToString(mask, 2);
    //    if (s.Length < size) s = s.PadLeft(size, '0');
    //    return s;
    //}

    private bool FindSwordfish(UnitType unitType, int candidate, Span<(int row, int col, int digit, int mask)> buffer)
    {
        // Erzeuge eine schreibgeschützte Snapshot-Matrix der Kandidaten (um Seiteneffekte zu vermeiden)
        int[,] snapshotMasks = new int[Puzzle.Size, Puzzle.Size];
        for (int r = 0; r < Puzzle.Size; r++)
        {
            for (int c = 0; c < Puzzle.Size; c++)
            {
                snapshotMasks[r, c] = _puzzle[r, c].SolverCandidates.BitMask;
            }
        }

        if (unitType == UnitType.Row)
        {
            Array.Clear(_rowMasks, 0, _rowMasks.Length); // alte Werte löschen
            for (int r = 0; r < Puzzle.Size; r++)
            {
                int m = 0;
                for (int c = 0; c < Puzzle.Size; c++)
                    if (_puzzle[r, c].Digit == 0 && (snapshotMasks[r, c] & (1 << (candidate - 1))) != 0)
                        m |= (1 << c);

                // originales Verhalten: alle Zeilen mit <= 3 Kandidaten berücksichtigen (einschließlich Singletons)
                if (ConstraintHelper.CountCandidate(m) is not > 3 && m != 0)
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
                    if (_puzzle[r, c].Digit == 0 && (snapshotMasks[r, c] & (1 << (candidate - 1))) != 0)
                        m |= (1 << r);

                if (ConstraintHelper.CountCandidate(m) is not > 3 && m != 0)
                    _colMasks[c] = (m, candidate);
            }
        }

        int bufferCount = 0;

        // Durchsuche 3er-Kombinationen (Swordfish)
        for (int i0 = 0; i0 < Puzzle.Size - 2; i0++)
            for (int i1 = i0 + 1; i1 < Puzzle.Size - 1; i1++)
                for (int i2 = i1 + 1; i2 < Puzzle.Size; i2++)
                {
                    int m0 = unitType == UnitType.Row ? _rowMasks[i0].mask : _colMasks[i0].mask;
                    int m1 = unitType == UnitType.Row ? _rowMasks[i1].mask : _colMasks[i1].mask;
                    int m2 = unitType == UnitType.Row ? _rowMasks[i2].mask : _colMasks[i2].mask;

                    if (m0 == 0 || m1 == 0 || m2 == 0)
                        continue;

                    int combinedMask = m0 | m1 | m2;

                    if (ConstraintHelper.CountCandidate(combinedMask) == 3)
                    {
                        // Debug: Basis-Info über das gefundene Pattern
                        //Debug.WriteLine($"[Swordfish] candidate={candidate} unitType={unitType} units=({i0},{i1},{i2}) masks: m0={MaskToBin(m0, Puzzle.Size)} m1={MaskToBin(m1, Puzzle.Size)} m2={MaskToBin(m2, Puzzle.Size)} combined={MaskToBin(combinedMask, Puzzle.Size)}");

                        // zusätzliche Absicherung: jede Spalte der Union muss in mindestens zwei der drei Units vorkommen
                        bool columnsHaveAtLeastTwoOccurrences = true;
                        //var columnOccurrencesSb = new StringBuilder();
                        for (int bit = 0; bit < Puzzle.Size; bit++)
                        {
                            int bitMask = 1 << bit;
                            if ((combinedMask & bitMask) == 0) continue;

                            int occurrences = 0;
                            if ((m0 & bitMask) != 0) occurrences++;
                            if ((m1 & bitMask) != 0) occurrences++;
                            if ((m2 & bitMask) != 0) occurrences++;

                            //columnOccurrencesSb.Append($"col{bit}:{occurrences} ");

                            if (occurrences < 2)
                            {
                                columnsHaveAtLeastTwoOccurrences = false;
                            }
                        }

                        //Debug.WriteLine($"[Swordfish] column occurrences: {columnOccurrencesSb}");

                        if (!columnsHaveAtLeastTwoOccurrences)
                        {
                            //Debug.WriteLine($"[Swordfish] Pattern rejected: not all columns appear at least twice.");
                            continue; // kein gültiger Swordfish nach engerer Definition
                        }

                        // Ermittle die Zellen, die zur Swordfish-Matrix gehören (Positionsliste in den 3 Units)
                        var participatingCells = new List<(int row, int col)>();
                        foreach (int bit in GetSetBits(combinedMask))
                        {
                            if (unitType == UnitType.Row)
                            {
                                if ((m0 & (1 << bit)) != 0) participatingCells.Add((i0, bit));
                                if ((m1 & (1 << bit)) != 0) participatingCells.Add((i1, bit));
                                if ((m2 & (1 << bit)) != 0) participatingCells.Add((i2, bit));
                            }
                            else // Column
                            {
                                if ((m0 & (1 << bit)) != 0) participatingCells.Add((bit, i0));
                                if ((m1 & (1 << bit)) != 0) participatingCells.Add((bit, i1));
                                if ((m2 & (1 << bit)) != 0) participatingCells.Add((bit, i2));
                            }
                        }

                        //Debug.WriteLine($"[Swordfish] participating cells: {string.Join(", ", participatingCells.Select(p => $"({p.row},{p.col})"))}");

                        // Sammle alle Eliminierungen in einer Liste (auf Snapshot-Basis, ohne sofortige Mutation)
                        var plannedRemovals = new List<(int row, int col)>();

                        foreach (int i in GetSetBits(combinedMask))
                        {
                            for (int j = 0; j < Puzzle.Size; j++)
                            {
                                if (j == i0 || j == i1 || j == i2) continue;

                                int rowIndex = unitType == UnitType.Row ? j : i;
                                int colIndex = unitType == UnitType.Row ? i : j;

                                // Nur Zellen mit dem Kandidaten in der Snapshot-Maske anvisieren
                                if ((snapshotMasks[rowIndex, colIndex] & (1 << (candidate - 1))) == 0) continue;

                                // Vermeide Duplikate
                                if (!plannedRemovals.Contains((rowIndex, colIndex)))
                                    plannedRemovals.Add((rowIndex, colIndex));
                            }
                        }

                        if (plannedRemovals.Count == 0)
                        {
                            //Debug.WriteLine($"[Swordfish] no planned removals for this pattern; skip.");
                            continue;
                        }

                        //Debug.WriteLine($"[Swordfish] planned removals: {string.Join(", ", plannedRemovals.Select(p => $"({p.row},{p.col})"))}");

                        // Validierungsphase: Prüfe, ob eine geplante Entfernung eine Zelle komplett ohne Kandidaten hinterlässt.
                        bool invalid = false;
                        foreach (var (rRem, cRem) in plannedRemovals)
                        {
                            int beforeMask = snapshotMasks[rRem, cRem];
                            int afterMask = beforeMask & ~(1 << (candidate - 1));
                            if (afterMask == 0)
                            {
                                // Diese Entfernung würde alle Kandidaten löschen -> unsicher, Pattern verwerfen
                                invalid = true;
                                //Debug.WriteLine($"[Swordfish] invalid removal would clear all candidates at ({rRem},{cRem}). Pattern rejected.");
                                break;
                            }
                        }

                        if (invalid)
                            continue; // Pattern verwerfen, keine Änderungen durchführen

                        int countRemovals = 0;
                        // Alles validiert -> Änderungen anwenden (einheitlich, und Buffer füllen)
                        foreach (var (rRem, cRem) in plannedRemovals)
                        {
                            if (bufferCount < buffer.Length)
                                buffer[bufferCount++] = (rRem, cRem, candidate, 0);

                            // Aktuelle Puzzle-Daten prüfen bevor wir entfernen (kann sich zwischen Constraints geändert haben)
                            var currentMask = _puzzle[rRem, cRem].SolverCandidates.BitMask;
                            if ((currentMask & (1 << (candidate - 1))) != 0)
                            {
                                _puzzle[rRem, cRem].SolverCandidates.Remove(candidate);
                                //Debug.WriteLine($"[Swordfish] removed candidate {candidate} from cell ({rRem}, {cRem})");
                                countRemovals++;
                            }
                        }

                        if (countRemovals > 0)
                            return true;
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
