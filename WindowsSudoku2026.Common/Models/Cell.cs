using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Common.Models;

public partial class Cell : ObservableObject, ICloneable
{
    public int Row { get; init; }
    public int Column { get; init; }
    [ObservableProperty] private int _digit;
    [ObservableProperty] private bool _isGiven;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isConflicting;
    [ObservableProperty] private bool _isHighlighted;
    [ObservableProperty] private Candidates _solverCandidates = new();
    [ObservableProperty] private Candidates _cornerCandidates = new();
    [ObservableProperty] private Candidates _centerCandidates = new();
    [ObservableProperty] private Candidates _conflictedCandidates = new();
    [ObservableProperty] private ObservableCollection<SudokuCellColor> _cellColors;

    public Cell(int row, int column)
    {
        Row = row;
        Column = column;
        _cornerCandidates = new();
        _centerCandidates = new();
        _conflictedCandidates = new Candidates();
        _cornerCandidates.Clear();
        _centerCandidates.Clear();
        _conflictedCandidates.Clear();
        _digit = 0;
        _isGiven = false;
        _isSelected = false;
        _isConflicting = false;
        _isHighlighted = false;
        _cellColors = new();
    }

    public override bool Equals(object? obj)
        => obj is Cell other && Row == other.Row && Column == other.Column;

    public override int GetHashCode()
        => HashCode.Combine(Row, Column);

    public object Clone()
    {
        return new Cell(Row, Column)
        {
            Digit = Digit,
            IsGiven = IsGiven,
            IsSelected = IsSelected,
            IsConflicting = IsConflicting,
            IsHighlighted = IsHighlighted,
            SolverCandidates = new Candidates(SolverCandidates.BitMask),
            CornerCandidates = new Candidates(CornerCandidates.BitMask),
            CenterCandidates = new Candidates(CenterCandidates.BitMask),
            ConflictedCandidates = new Candidates(ConflictedCandidates.BitMask),
            CellColors = [.. CellColors]
        };
    }
    public object CreateInitialStateClone()
    {
        return new Cell(Row, Column)
        {
            Digit = IsGiven ? Digit : 0,
            IsGiven = IsGiven,
            IsSelected = false,
            IsConflicting = false,
            IsHighlighted = false,
            SolverCandidates = IsGiven ? new(SolverCandidates.BitMask) : new(),
            CornerCandidates = new(0),
            CenterCandidates = new(0),
            ConflictedCandidates = new(0),
            CellColors = []
        };
    }

    public static bool operator ==(Cell? a, Cell? b) => Equals(a, b);
    public static bool operator !=(Cell? a, Cell? b) => !Equals(a, b);
}
