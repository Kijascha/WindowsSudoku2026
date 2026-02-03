using WindowsSudoku2026.Common.Attributes;

namespace WindowsSudoku2026.Common.DTO;

[Table("ColorPalettes")]
public class ColorPaletteDTOV2
{
    public int Id { get; set; }
    public string ColorPaletteString { get; set; } = string.Empty;
}
