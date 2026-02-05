using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Core.Interfaces;

namespace WindowsSudoku2026.Infrastructure.Repositories;

public partial class ColorPaletteRepository(IDtoSqlService dtoSqlService) : IColorPaletteRepository
{
    public async Task<bool> SavePaletteDto(ColorPaletteDTOV2 dto)
    {
        // Eigentlich unnötig aber sicher ist sicher
        bool exists = await dtoSqlService.ExistsAsync<ColorPaletteDTOV2>("Id", dto.Id);

        if (!exists)
        {
            await dtoSqlService.SaveAsync(dto);

            return true;
        }
        return false;
    }
    public async Task UpdatePalette(ColorPaletteDTOV2 dto)
    {
        await dtoSqlService.UpdateAsync(dto);
    }
    public async Task<ColorPaletteDTOV2?> GetPaletteDtoById(int id)
    {
        return await dtoSqlService.GetByIdAsync<ColorPaletteDTOV2>(id);
    }
    public async Task<IEnumerable<ColorPaletteDTOV2>> GetAllColorPaletteDtos()
    {
        return await dtoSqlService.GetAllAsync<ColorPaletteDTOV2>();
    }
    public async Task DeletePalette(int id)
    {
        await dtoSqlService.DeleteAsync<ColorPaletteDTOV2>(id);
    }
}
