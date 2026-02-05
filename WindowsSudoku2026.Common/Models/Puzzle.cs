using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using WindowsSudoku2026.Common.Utils.Colors;

namespace WindowsSudoku2026.Common.Models;

public class Puzzle : IPuzzle, INotifyPropertyChanged
{
    public const int Size = IPuzzle.Size;
    public int Id { get; set; }
    public string? Name { get; set; } // for holding the name of the custom puzzle
    public bool IsSolved { get; set; }
    private TimeSpan _timeSpent;
    public TimeSpan TimeSpent
    {
        get => _timeSpent;
        set
        {
            _timeSpent = value;
            OnPropertyChanged(nameof(TimeSpent));
        }

    }
    public int[,] Solution { get; set; }

    private BitmapSource? _bitmapSource;
    public BitmapSource? PreviewImage
    {
        get => _bitmapSource;
        set
        {
            _bitmapSource = value;
            OnPropertyChanged(nameof(PreviewImage));
        }
    } // for holding the preview image of the custom puzzle

    private readonly Cell[,] _board;

    private bool _isBatchUpdating = false;
    private ColorPalette _activePalette;
    public ColorPalette ActivePalette
    {
        get => _activePalette;
        set
        {
            _activePalette = value;
            OnPropertyChanged(nameof(ActivePalette));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public bool IsBatchUpdating => _isBatchUpdating;

    // Batch-Update Methode für Massenänderungen (z.B. Solver oder Board-Reset)
    public void BeginBatchUpdate() => _isBatchUpdating = true;

    public void EndBatchUpdate()
    {
        _isBatchUpdating = false;
        // Signalisiert der UI, dass sich ALLES geändert hat -> Ein einziger UI-Pass
        OnPropertyChanged(string.Empty);
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Puzzle(int id)
    {
        _activePalette = ColorPaletteFactory.CreateDefaultPalette();
        TimeSpent = TimeSpan.Zero;
        _board = new Cell[Size, Size];
        Solution = new int[Size, Size];
        Id = id;

        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                _board[row, col] = new Cell { Row = row, Column = col };
                Solution[row, col] = 0;
            }
        }
    }


    public Puzzle()
    {
        TimeSpent = TimeSpan.Zero;
        _activePalette = ColorPaletteFactory.CreateDefaultPalette();
        _board = new Cell[Size, Size];
        Solution = new int[Size, Size];

        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                _board[row, col] = new Cell { Row = row, Column = col };
                Solution[row, col] = 0;
            }
        }
    }

    public Cell this[int row, int column]
    {
        get
        {
            if ((uint)row >= Size) throw new ArgumentOutOfRangeException(nameof(row));
            if ((uint)column >= Size) throw new ArgumentOutOfRangeException(nameof(column));
            return _board[row, column];
        }
    }

    #region Validation
    public bool IsValidDigit(int row, int column, int digit)
    {
        if (digit == 0) return true;

        Cell currentCell = _board[row, column];
        // Nutze die Span-Methoden, die du schon hast!
        if (ContainsDigit(GetRowSpan(row), digit, currentCell)) return false;
        if (ContainsDigit(GetColumnSpan(column), digit, currentCell)) return false;
        if (ContainsDigit(GetBoxSpan(row / 3, column / 3), digit, currentCell)) return false;

        return true;
    }

    private static bool ContainsDigit(ReadOnlySpan<Cell> unit, int digit, Cell ignoreCell)
    {
        foreach (var cell in unit)
        {
            // ignoreIndex wird genutzt, um die aktuelle Zelle selbst zu überspringen
            if (!cell.Equals(ignoreCell) && cell.Digit == digit) return true;
        }
        return false;
    }
    #endregion

    #region Get Units
    // Allocation-free for rows (backed directly by the multidimensional array memory)
    public ReadOnlySpan<Cell> GetRowSpan(int row)
    {
        if ((uint)row >= Size) throw new ArgumentOutOfRangeException(nameof(row));
        ref Cell start = ref _board[row, 0];
        return MemoryMarshal.CreateReadOnlySpan(ref start, Size);
    }
    // Columns are not laid out contiguously for a given column, so we copy into a temporary array.
    // This causes a short-lived heap allocation per call.
    public ReadOnlySpan<Cell> GetColumnSpan(int column)
    {
        if ((uint)column >= Size) throw new ArgumentOutOfRangeException(nameof(column));
        var arr = new Cell[Size];
        for (int r = 0; r < Size; r++)
        {
            arr[r] = _board[r, column];
        }
        return arr;
    }
    // Boxes are 3x3 and not contiguous as a single run, so we copy into a temporary array (heap allocation).
    public ReadOnlySpan<Cell> GetBoxSpan(int boxRow, int boxCol)
    {
        if ((uint)boxRow >= 3) throw new ArgumentOutOfRangeException(nameof(boxRow));
        if ((uint)boxCol >= 3) throw new ArgumentOutOfRangeException(nameof(boxCol));

        var arr = new Cell[9];
        int idx = 0;
        int startRow = boxRow * 3;
        int startCol = boxCol * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                arr[idx++] = _board[r, c];
            }
        }

        return arr;
    }
    // Optional helper: box by linear index
    public ReadOnlySpan<Cell> GetBoxSpan(int boxIndex)
    {
        if ((uint)boxIndex >= Size) throw new ArgumentOutOfRangeException(nameof(boxIndex));
        int boxRow = boxIndex / 3;
        int boxCol = boxIndex % 3;
        return GetBoxSpan(boxRow, boxCol);
    }
    #endregion

    #region Converting and Copying
    public Puzzle Clone()
    {
        // ID übernehmen!
        var clone = new Puzzle(Id)
        {
            Name = Name,
            PreviewImage = PreviewImage,
            ActivePalette = ActivePalette// falls Clone existiert
        };

        // Lösung kopieren
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                clone.Solution[r, c] = Solution[r, c];
            }
        }

        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                Cell original = _board[row, col];
                Cell copy = new Cell
                {
                    Row = original.Row,
                    Column = original.Column,
                    Digit = original.Digit,

                    SolverCandidates = new(original.SolverCandidates.BitMask),
                    CenterCandidates = new(original.CenterCandidates.BitMask),
                    CornerCandidates = new(original.CornerCandidates.BitMask),

                    IsGiven = original.IsGiven,
                    IsSelected = original.IsSelected,
                    IsHighlighted = original.IsHighlighted,
                    IsConflicting = original.IsConflicting
                };

                // Farben kopieren
                foreach (var color in original.CellColors)
                    copy.CellColors.Add(color);

                clone._board[row, col] = copy;
            }
        }

        return clone;
    }
    public Puzzle CreateInitialStateCopy()
    {
        // ID und Metadaten übernehmen
        var reset = new Puzzle(Id)
        {
            Name = Name,
            PreviewImage = PreviewImage,
            ActivePalette = ActivePalette,
            IsSolved = false,           // Zwingend auf false setzen
            TimeSpent = TimeSpan.Zero          // Zeit auf 0 setzen
        };

        // Lösung kopieren (bleibt identisch)
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                reset.Solution[r, c] = Solution[r, c];
            }
        }

        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                Cell original = _board[row, col];

                // Neue Cell erstellen
                Cell copy = new Cell
                {
                    Row = original.Row,
                    Column = original.Column,
                    IsGiven = original.IsGiven,

                    // NUR die Digit übernehmen, wenn sie ein 'Given' ist
                    Digit = original.IsGiven ? original.Digit : 0,

                    // Kandidaten/Marks werden NICHT übernommen (leerer Zustand)
                    SolverCandidates = original.IsGiven ? new(original.SolverCandidates.BitMask) : new(),
                    CenterCandidates = new(0),
                    CornerCandidates = new(0),

                    // Status-Flags zurücksetzen
                    IsSelected = false,
                    IsHighlighted = false,
                    IsConflicting = false
                };

                // Farben nur übernehmen, wenn sie zum Initial-Design gehören (meist leer)
                // Falls du Farben als User-Markierung nutzt, hier .Clear() lassen

                reset._board[row, col] = copy;
            }
        }

        return reset;
    }
    #endregion
}
