using System.Windows.Media;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Common.Utils;

public static class SudokuColorPalette
{
    public static readonly IReadOnlyDictionary<SudokuCellColor, Color> ColorPalette =
        new Dictionary<SudokuCellColor, Color>
        {
        { SudokuCellColor.None, System.Windows.Media.Colors.White },
        { SudokuCellColor.Color1, Color.FromRgb(249, 137, 135) }, // red
        { SudokuCellColor.Color2, Color.FromRgb(253, 243, 140) }, // gelb 
        { SudokuCellColor.Color3, Color.FromRgb(139, 194, 249) }, // blau 
        { SudokuCellColor.Color4, Color.FromRgb(209, 239, 166) }, // green
        { SudokuCellColor.Color5, Color.FromRgb(241, 176, 247) }, // purple
        { SudokuCellColor.Color6, Color.FromRgb(239, 192, 132) }, // orange
        { SudokuCellColor.Color7, Color.FromRgb(230, 230, 230) }, // hellgrau
        { SudokuCellColor.Color8, Color.FromRgb(176, 176, 176) }, // grau
        { SudokuCellColor.Color9, Color.FromRgb(102, 102, 102) }, // schwarz
        };

    public static Brush GetBrush(SudokuCellColor color, byte alpha = 255)
    {
        var c = ColorPalette[color];
        return new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
    }
}
