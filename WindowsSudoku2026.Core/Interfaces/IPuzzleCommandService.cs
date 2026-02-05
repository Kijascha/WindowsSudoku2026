using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Core.Interfaces;

public interface IPuzzleCommandService
{
    void RemoveCandidates(IPuzzle? currentPuzzle, CandidateType type);
    void RemoveColors(IPuzzle? currentPuzzle);
    void RemoveDigits(IPuzzle? currentPuzzle, GameType gameType);
    void SmartRemovalFromSelected(IPuzzle? currentPuzzle, GameType gameType);
    void UpdateCandidates(IPuzzle? currentPuzzle, int candidate, InputActionType selectedInputActionType);
    void UpdateColors(IPuzzle? currentPuzzle, int colorCode);
    void UpdateDigits(IPuzzle? currentPuzzle, int digit, GameType gameType = GameType.Play);
}