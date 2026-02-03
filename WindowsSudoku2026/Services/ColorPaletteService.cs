using CommunityToolkit.Mvvm.ComponentModel;
using WindowsSudoku2026.Common.DTO;

namespace WindowsSudoku2026.Services;

public partial class ColorPaletteService(IDtoSqlService dtoSqlService) : ObservableObject, IColorPaletteService
{
    private readonly IDtoSqlService _dtoSqlService = dtoSqlService;

    [ObservableProperty] private ColorPaletteDTOV2? _selectedColorPaletteDto;

    public async Task<bool> SavePaletteDto(ColorPaletteDTOV2 dto)
    {
        // Eigentlich unnötig aber sicher ist sicher
        bool exists = await _dtoSqlService.ExistsAsync<ColorPaletteDTOV2>("Id", dto.Id);

        if (!exists)
        {
            await _dtoSqlService.SaveAsync<ColorPaletteDTOV2>(dto);

            return true;
        }
        return false;
    }
    public async Task UpdatePalette(ColorPaletteDTOV2 dto)
    {
        await _dtoSqlService.UpdateAsync(dto);
    }
    public async Task<ColorPaletteDTOV2?> GetPaletteDtoById(int id)
    {
        return await _dtoSqlService.GetByIdAsync<ColorPaletteDTOV2>(id);
    }
    public async Task<IEnumerable<ColorPaletteDTOV2>> GetAllColorPaletteDtos()
    {
        return await _dtoSqlService.GetAllAsync<ColorPaletteDTOV2>();
    }
    public async Task DeletePalette(int id)
    {
        await _dtoSqlService.DeleteAsync<ColorPaletteDTOV2>(id);
    }
}
