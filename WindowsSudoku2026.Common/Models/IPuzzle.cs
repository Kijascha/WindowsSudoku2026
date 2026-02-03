using System.ComponentModel;
using System.Windows.Media.Imaging;
using WindowsSudoku2026.Common.Utils.Colors;

namespace WindowsSudoku2026.Common.Models
{
    public interface IPuzzle
    {
        public const int Size = 9;
        event PropertyChangedEventHandler? PropertyChanged;
        ColorPalette ActivePalette { get; set; }
        int Id { get; set; }
        string? Name { get; set; }
        int[,] Solution { get; set; }
        bool IsSolved { get; set; }
        TimeSpan TimeSpent { get; set; }
        BitmapSource? PreviewImage { get; set; } // for holding the preview image of the custom puzzle
        public bool IsBatchUpdating { get; }
        Cell this[int row, int column] { get; }
        Puzzle Clone();
        Puzzle CreateInitialStateCopy();
        ReadOnlySpan<Cell> GetBoxSpan(int boxIndex);
        ReadOnlySpan<Cell> GetBoxSpan(int boxRow, int boxCol);
        ReadOnlySpan<Cell> GetColumnSpan(int column);
        ReadOnlySpan<Cell> GetRowSpan(int row);
        bool IsValidDigit(int row, int column, int digit);
        public void BeginBatchUpdate();
        public void EndBatchUpdate();
    }
}