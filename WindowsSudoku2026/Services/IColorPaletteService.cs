using WindowsSudoku2026.Common.DTO;

namespace WindowsSudoku2026.Services
{
    public interface IColorPaletteService
    {
        ColorPaletteDTOV2? SelectedColorPaletteDto { get; set; }
        Task DeletePalette(int id);
        Task<IEnumerable<ColorPaletteDTOV2>> GetAllColorPaletteDtos();
        Task<ColorPaletteDTOV2?> GetPaletteDtoById(int id);
        Task<bool> SavePaletteDto(ColorPaletteDTOV2 dto);
        Task UpdatePalette(ColorPaletteDTOV2 dto);
    }
}