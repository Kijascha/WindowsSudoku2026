using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Core.Interfaces;

namespace WindowsSudoku2026.Infrastructure.Repositories;

public class SudokuRepository(IDtoSqlService dtoSqlService) : ISudokuRepository
{
    public async Task<bool> SaveAsync(PuzzleDTO dto)
    {
        try
        {
            int id = await dtoSqlService.SaveAsync(dto);
            dto.Id = id;
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> UpdatePuzzleAsync(PuzzleDTO dto)
    {
        try
        {
            await dtoSqlService.UpdateAsync(dto);
            return true;
        }
        catch { return false; }
    }
    public async Task<PuzzleDTO?> GetPuzzleDtoByIdAsync(int id)
        => await dtoSqlService.GetByIdAsync<PuzzleDTO>(id);
    public async Task<IEnumerable<PuzzleDTO>> GetAvailablePuzzlesAsync()
        => await dtoSqlService.GetAllAsync<PuzzleDTO>(orderBy: "Name ASC");
    public async Task DeletePuzzleById(int id)
        => await dtoSqlService.DeleteAsync<PuzzleDTO>(id);
    public async Task<bool> NameExistsAsync(string name)
        => await dtoSqlService.ExistsAsync<PuzzleDTO>("Name", name);
}
