using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Utils.Colors;

namespace WindowsSudoku2026.Core.Factories;

public class SudokuFactory
{
    public Puzzle CreateEmptyPuzzle()
    {
        return new Puzzle()
        {
            Id = 0,
            Name = string.Empty,
            TimeSpent = TimeSpan.Zero,
            PreviewImage = null,
            ActivePalette = ColorPaletteFactory.CreateDefaultPalette()
        };
    }
}
