using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Core.Interfaces
{
    public interface IGameServiceV2
    {
        IPuzzle? CurrentPuzzle { get; set; }
        IPuzzleCommandService PuzzleCommandService { get; }
        ITimerService Timer { get; }

        Task<bool> CreateAndSaveNewPuzzle(bool useAutoNaming, bool alwaysAppendAutoSuffix, string defaultPrefix, NamingStrategy namingStrategy);
        Task SyncAndSaveCurrentProgressAsync();
        Task<IEnumerable<PuzzleDTO>> GetAvailablePuzzlesAsync();
        Task<IPuzzle?> LoadPuzzleByIdAsync(int puzzleId);
        Task DeletePuzzleAsync(PuzzleDTO puzzle);
        Task<PuzzleDTO?> ResetCurrentPuzzleAsync(IPuzzle? selectedPuzzle);

        Task<bool> VerifyAndFinishPuzzle();
        bool IsPuzzleSolvable();
    }
}