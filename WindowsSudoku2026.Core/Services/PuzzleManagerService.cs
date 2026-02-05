using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Records;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.Mapper.DTO;
using WindowsSudoku2026.Solver.ConstraintSolver;

namespace WindowsSudoku2026.Core.Services;

public class PuzzleManagerService(ISudokuRepository repository, ITimerService timerService) : IPuzzleManagerService
{
    public async Task<bool> PrepareAndSaveNewPuzzleAsync(IPuzzle puzzle, NamingOptions options)
    {
        // 1. Logik: Lösung generieren
        if (!CreateAndSaveSolution(puzzle)) return false;

        // 2. Logik: Namen generieren
        puzzle.Name = await GenerateUniqueNameAsync(puzzle.Name ?? string.Empty, options);

        // 3. Infrastruktur: Nur noch speichern
        return await repository.SaveAsync(DtoMapper.MapToDto(puzzle));
    }
    public async Task<IPuzzle?> LoadPuzzleByIdAsync(int puzzleId, Action<int, int, int, GameType> onUpdateDigit)
    {
        // 1. Gezieltes Laden via Repository
        var dto = await repository.GetPuzzleDtoByIdAsync(puzzleId);
        if (dto == null) return null;

        // 2. Mapping von DTO zu Logik-Objekt
        IPuzzle newPuzzle = DtoMapper.MapFromDto(dto);

        // 3. UI-Zellen aktualisieren via Callback
        // So bleibt der Service frei von WPF-Abhängigkeiten, triggert aber die UI
        for (int r = 0; r < IPuzzle.Size; r++) // Annahme: Size ist 9
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                onUpdateDigit(r, c, newPuzzle[r, c].Digit, GameType.Play);
            }
        }

        // 4. Timer initialisieren
        timerService.Start(newPuzzle.TimeSpent);

        return newPuzzle;
    }

    public async Task<IPuzzle?> LoadPuzzleByIdAsync(int puzzleId)
    {
        // 1. Gezieltes Laden via Repository
        var dto = await repository.GetPuzzleDtoByIdAsync(puzzleId);
        if (dto == null) return null;

        // 2. Mapping von DTO zu Logik-Objekt
        IPuzzle newPuzzle = DtoMapper.MapFromDto(dto);

        // 3. Timer initialisieren
        timerService.Start(newPuzzle.TimeSpent);

        return newPuzzle;
    }

    private static bool CreateAndSaveSolution(IPuzzle currentPuzzle)
    {
        // Mit einer Kopie vom Puzzle Arbeiten, wir wollen das puzzle selbst ja nicht damit lösen
        Puzzle puzzleToSolve = currentPuzzle.Clone();
        // Solver vorbereiten
        ConstraintSolver _constraintSolver = new(puzzleToSolve);

        // Solve ausführen
        bool success = _constraintSolver.Solve();

        if (success)
        {
            // puzzleToSolve in die Solution schreiben:

            for (int r = 0; r < IPuzzle.Size; r++)
            {
                for (int c = 0; c < IPuzzle.Size; c++)
                {
                    currentPuzzle.Solution[r, c] = puzzleToSolve[r, c].Digit;
                }
            }

            return true;
        }
        return false;
    }
    // Diese Methode gehört in deinen PuzzleService (Core)
    private async Task<string> GenerateUniqueNameAsync(string currentName, NamingOptions options)
    {
        bool nameIsEmpty = string.IsNullOrWhiteSpace(currentName);

        string candidate = string.Empty;

        // Falls Zeitstempel-Strategie gewählt wurde
        if (options.UseAutoNaming && (nameIsEmpty || options.AlwaysAppendAutoSuffix))
        {
            // Bestimme den Basis-Namen (Präfix oder aktueller Name)
            string baseName = nameIsEmpty ? options.DefaultPrefix : currentName;

            if (options.PreferredStrategy == NamingStrategy.Timestamp)
            {
                // Beispiel: "Sudoku_20260127_1045"
                return $"{options.DefaultPrefix}_{DateTime.Now:yyyyMMdd_HHmm}";
            }

            // Counter-Logik: Nutzt die Datenbank für den Check
            int count = 1;
            bool exists;

            do
            {
                candidate = $"{options.DefaultPrefix}{count++}";
                // Wir fragen die DB gezielt: "Gibt es diesen Namen schon?"
                exists = await repository.NameExistsAsync(candidate);
            }
            while (exists);
        }

        return candidate;
    }
    public async Task<bool> UpdatePuzzleAsync(PuzzleDTO dto)
        => await repository.UpdatePuzzleAsync(dto);
    public async Task DeletePuzzleDTO(PuzzleDTO puzzle)
        => await repository.DeletePuzzleById(puzzle.Id);
    public async Task<IEnumerable<PuzzleDTO>> GetAvailablePuzzlesAsync()
        => await repository.GetAvailablePuzzlesAsync();
}
