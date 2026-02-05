using WindowsSudoku2026.Common.DTO;

namespace WindowsSudoku2026.Core.Interfaces;

public interface ISudokuRepository
{
    Task<bool> SaveAsync(PuzzleDTO dto);
    Task<bool> UpdatePuzzleAsync(PuzzleDTO dto);
    Task<PuzzleDTO?> GetPuzzleDtoByIdAsync(int id);
    Task<IEnumerable<PuzzleDTO>> GetAvailablePuzzlesAsync();
    Task<bool> NameExistsAsync(string name);
    Task DeletePuzzleById(int id);
}
