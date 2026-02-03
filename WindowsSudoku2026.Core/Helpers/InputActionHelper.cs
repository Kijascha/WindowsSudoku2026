using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Core.Helpers;

public static class InputActionHelper
{
    public static bool IsFilledOnInputAction(IPuzzle puzzle, InputActionType inputActionType, GameType gameType)
    {
        // Zentrale Prüfung nach jeder Eingabe
        if (inputActionType == InputActionType.Digits && gameType == GameType.Play)
        {
            // Greife auf das Puzzle-Objekt zu (Annahme: du hast eine Referenz oder DP dafür)
            if (puzzle == null) return false;

            bool filled = true;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (puzzle[r, c].Digit == 0)
                    {
                        filled = false;
                        break;
                    }
                }
                if (!filled) break;
            }

            return filled;
        }
        return false;
    }
}
