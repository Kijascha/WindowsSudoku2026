using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using WindowsSudoku2026.Common.Enums;

namespace WindowsSudoku2026.Common.Utils.Colors;

public partial class SudokuColor : ObservableObject
{
    [ObservableProperty] private SudokuCellColor _key;
    [ObservableProperty] private Color _value;
    public SudokuColor(SudokuCellColor key, Color value)
    {
        _key = key;
        _value = value;
    }
    public SudokuColor()
    {
        _key = SudokuCellColor.None;
        _value = System.Windows.Media.Colors.White;
    }
    override public bool Equals(object? obj)
    {
        if (obj is not SudokuColor other) return false;
        return Key == other.Key && Value.Equals(other.Value);
    }
    override public int GetHashCode() => HashCode.Combine(Key, Value);
    override public string ToString() => $"{Key}: {Value}";
    public SudokuColor Clone()
    {
        return new SudokuColor(Key, Value);
    }
}
public partial class ColorPalette : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private ObservableCollection<SudokuColor> _sudokuColors;

    public ColorPalette(ObservableCollection<SudokuColor> colors)
    {
        _sudokuColors = new ObservableCollection<SudokuColor>(colors); // Kopie sichern
    }

    public SudokuColor this[int key]
    {
        get => SudokuColors[key];
        set => SudokuColors[key] = value;
    }
    public ColorPalette Clone()
    {
        var clone = new ColorPalette(SudokuColors);

        foreach (var c in SudokuColors)
            clone.SudokuColors.Add(c.Clone());

        return clone;
    }
}
