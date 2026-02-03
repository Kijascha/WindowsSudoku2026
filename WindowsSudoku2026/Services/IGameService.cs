using System.Collections.ObjectModel;
using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Services;

public interface IGameService
{
    IPuzzle CurrentPuzzle { get; set; }
    ITimerService Timer { get; }

    Task<ObservableCollection<PuzzleDTO>> GetAvailablePuzzlesAsync();
    void SyncTimeWithModel();
    void CreateNewPuzzle();
    bool SolvePuzzle();
    bool IsPuzzleSolvable();
    Task<bool> VerifyAndFinishPuzzle();
    Task<bool> SaveNewCustomPuzzleAsync(bool useAutoNaming, bool alwaysAppendAutoSuffix, string defaultNamingPrefix, NamingStrategy preferredNamingStrategy);
    Task SaveCurrentPuzzleProgressAsync();
    Task<bool> LoadPuzzleByIdAsync(int puzzleId);
    Task DeletePuzzleAsync(int id);
    Task<PuzzleDTO?> ResetCurrentPuzzleAsync(IPuzzle? selectedPuzzle);
    void SmartRemovalFromSelected();
    void RemoveCandidates(CandidateType type);
    void RemoveColors();
    void RemoveDigits();
    void UpdateCandidates(int candidate, InputActionType selectedInputActionType);
    void UpdateColors(int colorCode);
    void UpdateDigits(int digit, GameType gameType = GameType.Play);
}