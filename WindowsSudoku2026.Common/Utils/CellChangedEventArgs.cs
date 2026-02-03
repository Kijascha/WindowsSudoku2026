using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Common.Utils;

public sealed class CellChangedEventArgs : EventArgs
{
    public Cell? Cell { get; }
    public string? PropertyName { get; }

    public CellChangedEventArgs(Cell? cell, string? propertyName)
    {
        Cell = cell;
        PropertyName = propertyName;
    }
}
