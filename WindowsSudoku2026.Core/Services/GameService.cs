using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.Mapper.DTO;

namespace WindowsSudoku2026.Core.Services;

public class GameServiceV2(
    IPuzzleManagerService puzzleManagerService,
    IPuzzleCommandService puzzleCommandService,
    ITimerService timer) : IGameServiceV2
{
    private readonly IPuzzleManagerService _puzzleManagerService = puzzleManagerService;
    public IPuzzle? CurrentPuzzle { get; set; }

    public IPuzzleCommandService PuzzleCommandService { get; } = puzzleCommandService;
    public ITimerService Timer { get; } = timer;

    public async Task<IEnumerable<PuzzleDTO>> GetAvailablePuzzlesAsync()
        => await _puzzleManagerService.GetAvailablePuzzlesAsync();
    public async Task<bool> CreateAndSaveNewPuzzle(bool useAutoNaming, bool alwaysAppendAutoSuffix, string defaultPrefix, NamingStrategy namingStrategy)
    {
        if (CurrentPuzzle == null) return false;

        return await _puzzleManagerService.PrepareAndSaveNewPuzzleAsync(CurrentPuzzle, new(useAutoNaming, alwaysAppendAutoSuffix, defaultPrefix, namingStrategy));
    }
    public async Task SyncAndSaveCurrentProgressAsync()
    {
        if (CurrentPuzzle == null) return;

        Timer.Pause();
        CurrentPuzzle.TimeSpent = Timer.ElapsedTime;
        await _puzzleManagerService.UpdatePuzzleAsync(DtoMapper.MapToDto(CurrentPuzzle));
    }

    public Task<IPuzzle?> LoadPuzzleByIdAsync(int puzzleId) =>
        _puzzleManagerService.LoadPuzzleByIdAsync(puzzleId);

    public async Task DeletePuzzleAsync(PuzzleDTO puzzle)
    {
        // Nutzt die generische Methode, die wir vorhin im DtoSqlService gebaut haben
        await _puzzleManagerService.DeletePuzzleDTO(puzzle);
    }
    public async Task<PuzzleDTO?> ResetCurrentPuzzleAsync(IPuzzle? selectedPuzzle)
    {
        if (selectedPuzzle == null) return null;

        // Erstelle die saubere Kopie
        var resetPuzzle = selectedPuzzle.CreateInitialStateCopy();

        // Timer zurücksetzen
        Timer.Pause();
        Timer.Reset();

        PuzzleDTO mappedResetPuzzle = DtoMapper.MapToDto(resetPuzzle);
        // In der DB speichern (DtoSqlService nutzt die Id der Kopie für den Update)
        await _puzzleManagerService.UpdatePuzzleAsync(mappedResetPuzzle);

        // WICHTIG: Das CurrentPuzzle im Service austauschen
        return mappedResetPuzzle;
    }

    #region Validation
    public async Task<bool> VerifyAndFinishPuzzle()
    {
        bool isPuzzleCorrect = IsPuzzleCorrect();

        if (isPuzzleCorrect && CurrentPuzzle != null)
        {
            CurrentPuzzle.IsSolved = true;
            await SyncAndSaveCurrentProgressAsync();

            return true;
        }
        return false;
    }
    private bool IsPuzzleCorrect()
    {
        if (CurrentPuzzle == null) return false;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                if (CurrentPuzzle[r, c].Digit != CurrentPuzzle.Solution[r, c]) return false;
            }
        }
        return true;
    }
    public bool IsPuzzleSolvable()
    {
        if (CurrentPuzzle == null) return false;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                if (CurrentPuzzle[r, c].Digit == 0) continue;
                if (CurrentPuzzle[r, c].Digit != CurrentPuzzle.Solution[r, c]) return false;
            }
        }
        return true;

    }
    #endregion
}
