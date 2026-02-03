using System.Numerics;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Solver.ConstraintSolver.Utils;

internal static class ConstraintHelper
{
    public static int CombineUnits(params int[] unitIndices)
    {
        var combinedUnitMask = 0;
        foreach (int unitIndex in unitIndices) combinedUnitMask |= (1 << unitIndex);
        return combinedUnitMask;
    }
    public static int CountUnits(int[] unitMasks, int unitIndex, int combinedunitMask) =>
         BitOperations.PopCount((uint)(unitMasks[unitIndex] & combinedunitMask));
    public static int CountUnits(int unitMask) => BitOperations.PopCount((uint)unitMask);
    public static int CountCandidate(int candidateMask) => BitOperations.PopCount((uint)candidateMask);
    public static bool HasMoreCandidatesThanAllowed(int[] unitMasks, int allowed, int allowedMask, params int[] units)
    {
        bool result = false;
        foreach (int unit in units)
        {
            if ((CountUnits(unitMasks[unit]) != allowed) || ((unitMasks[unit] & ~allowedMask) != 0))
                return true;
        }
        return result;
    }
    public static int CountUnitsInUnit(int[] unitMasks, int combinedunitMask, params int[] units)
    {
        int result = 0;
        foreach (int unit in units)
        {
            result += CountUnits(unitMasks, unit, combinedunitMask);
        }
        return result;
    }
    public static bool IsCandidateAlreadySolved(IPuzzle puzzle, Cell cell, int candidate)
    {
        // Prüfe Row
        foreach (var peer in puzzle.GetRowSpan(cell.Row))
            if (peer.Digit == candidate) return true;

        // Prüfe Column
        foreach (var peer in puzzle.GetColumnSpan(cell.Column))
            if (peer.Digit == candidate) return true;

        // Prüfe Box
        foreach (var peer in puzzle.GetBoxSpan(cell.Row / 3, cell.Column / 3))
            if (peer.Digit == candidate) return true;

        return false;
    }
    public static bool HasFixedDigitInRowsOrColsInline(IPuzzle puzzle, int[] rows, int[] cols)
    {
        if (rows.Length != cols.Length) return false;

        var result = false;
        foreach (var row in rows)
            foreach (var col in cols)
                result |= (puzzle[row, col].Digit != 0);

        return result;
    }
}
