using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Records;

namespace WindowsSudoku2026.Core.Interfaces;

public interface IPuzzleManagerService
{
    Task<IPuzzle?> LoadPuzzleByIdAsync(int puzzleId);
    Task<IPuzzle?> LoadPuzzleByIdAsync(int puzzleId, Action<int, int, int, GameType> onUpdateDigit);
    Task<bool> PrepareAndSaveNewPuzzleAsync(IPuzzle puzzle, NamingOptions options);
    Task<bool> UpdatePuzzleAsync(PuzzleDTO dto);
    Task DeletePuzzleDTO(PuzzleDTO puzzle);
    Task<IEnumerable<PuzzleDTO>> GetAvailablePuzzlesAsync();
}