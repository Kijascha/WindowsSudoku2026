using WindowsSudoku2026.Common.DTO;

namespace WindowsSudoku2026.Core.Interfaces;

public interface IColorPaletteRepository
{
    Task DeletePalette(int id);
    Task<IEnumerable<ColorPaletteDTOV2>> GetAllColorPaletteDtos();
    Task<ColorPaletteDTOV2?> GetPaletteDtoById(int id);
    Task<bool> SavePaletteDto(ColorPaletteDTOV2 dto);
    Task UpdatePalette(ColorPaletteDTOV2 dto);
}
