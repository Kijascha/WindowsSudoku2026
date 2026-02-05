using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Common.Models;

public partial class Cell : ObservableObject
{
    public int Row { get; set; }
    public int Column { get; set; }
    [ObservableProperty] private int _digit;
    [ObservableProperty] private bool _isGiven;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isConflicting;
    [ObservableProperty] private bool _isHighlighted;
    [ObservableProperty] private Candidates _solverCandidates = new();
    [ObservableProperty] private Candidates _cornerCandidates = new();
    [ObservableProperty] private Candidates _centerCandidates = new();
    [ObservableProperty] private ObservableCollection<SudokuCellColor> _cellColors;

    public Cell()
    {
        _cornerCandidates = new();
        _centerCandidates = new();
        _cornerCandidates.Clear();
        _centerCandidates.Clear();
        _digit = 0;
        _isGiven = false;
        _isSelected = false;
        _isConflicting = false;
        _isHighlighted = false;
        _cellColors = new();
    }
    // Override Equals method
    public override bool Equals(object? obj)
    {
        // Null check
        if (obj == null)
        {
            return false;
        }

        // Reference equality check
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        // Type check
        if (obj.GetType() != typeof(Cell))
        {
            return false;
        }

        // Cast the object to Cell and compare Row and Col
        var otherCell = (Cell)obj;
        return this.Row == otherCell.Row && this.Column == otherCell.Column;
    }

    // Override GetHashCode method
    public override int GetHashCode()
    {
        // Use a prime number to combine hash codes for Row and Col
        return (Row * 397) ^ Column;
    }
}
