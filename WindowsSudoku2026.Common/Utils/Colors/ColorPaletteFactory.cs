using System.Collections.ObjectModel;
using System.Windows.Media;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Common.Utils.Colors;

public static class ColorPaletteFactory
{
    public static ColorPalette CreateDefaultPalette()
    {
        return new ColorPalette(new ObservableCollection<SudokuColor>
        {
            { new SudokuColor(SudokuCellColor.Color1, Color.FromRgb(249, 137, 135)) }, // red
            { new SudokuColor(SudokuCellColor.Color2, Color.FromRgb(253, 243, 140)) }, // gelb 
            { new SudokuColor(SudokuCellColor.Color3, Color.FromRgb(139, 194, 249)) }, // blau 
            { new SudokuColor(SudokuCellColor.Color4, Color.FromRgb(209, 239, 166)) }, // green
            { new SudokuColor(SudokuCellColor.Color5, Color.FromRgb(241, 176, 247)) }, // purple
            { new SudokuColor(SudokuCellColor.Color6, Color.FromRgb(239, 192, 132)) }, // orange
            { new SudokuColor(SudokuCellColor.Color7, Color.FromRgb(230, 230, 230)) }, // hellgrau
            { new SudokuColor(SudokuCellColor.Color8, Color.FromRgb(176, 176, 176)) }, // grau
            { new SudokuColor(SudokuCellColor.Color9, Color.FromRgb(102, 102, 102)) }, // schwarz
        });
    }
    public static ColorPalette CreateBlankPalette()
    {
        return new ColorPalette(new ObservableCollection<SudokuColor>
        {
            { new SudokuColor(SudokuCellColor.Color1, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color2, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color3, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color4, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color5, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color6, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color7, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color8, Color.FromRgb(255, 255, 255)) },
            { new SudokuColor(SudokuCellColor.Color9, Color.FromRgb(255, 255, 255)) },
        });
    }
    public static Brush GetBrush(ColorPalette palette, SudokuCellColor key, byte alpha = 255)
    {
        var colorIndex = palette.SudokuColors.Index().Where(c => c.Item.Key == key).Select(c => c.Index).First();
        var c = palette[colorIndex].Value;
        return new SolidColorBrush(Color.FromArgb(alpha, c.R, c.G, c.B));
    }
}
