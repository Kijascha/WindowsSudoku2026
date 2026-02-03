using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Records;
using WindowsSudoku2026.Common.Utils;
using WindowsSudoku2026.DTO;
using WindowsSudoku2026.Essential;
using WindowsSudoku2026.Solver.ConstraintSolver;

namespace WindowsSudoku2026.Services;

public partial class GameService : ObservableObject, IGameService, IRecipient<PuzzleSelectedMessage>
{
    private readonly IAppPaths _appPaths;
    private readonly IDtoJsonService _dtoJsonService;
    private readonly IDtoSqlService _dtoSqlService;
    private readonly ITimerService _timerService;

    [ObservableProperty] private IPuzzle _currentPuzzle;

    // Wir leiten die Eigenschaft für das UI einfach weiter
    public ITimerService Timer => _timerService;

    public GameService(
        IAppPaths appPaths,
        IDtoJsonService dtoJsonService,
        IDtoSqlService dtoSqlService,
        ITimerService timerService)
    {
        _currentPuzzle = new Puzzle();
        _appPaths = appPaths;
        _dtoJsonService = dtoJsonService;
        _dtoSqlService = dtoSqlService;
        _timerService = timerService;
        WeakReferenceMessenger.Default.Register(this);

    }
    partial void OnCurrentPuzzleChanged(IPuzzle value)
    {
        Debug.WriteLine("Puzzle Changed");
    }
    public void Receive(PuzzleSelectedMessage message)
    {
        // 1. Mapping
        var newPuzzle = DtoMapper.MapFromDto(message.SelectedPuzzle);

        // 2. WICHTIG: Falls das Mapping die Zellen einzeln befüllt, 
        // löst das oft noch keine Events aus. 
        // Wir triggern das globale Event deiner Puzzle-Klasse:
        newPuzzle.BeginBatchUpdate();

        // 3. Zuweisen
        CurrentPuzzle = newPuzzle;

        // 4. Signal an die UI senden (OnPropertyChanged(string.Empty))
        // Das zwingt die View, ALLES unterhalb von CurrentPuzzle neu zu prüfen.
        CurrentPuzzle.EndBatchUpdate();

        // 5. Zusätzlich dem PlayViewModel bescheid geben
        OnPropertyChanged(nameof(CurrentPuzzle));
    }
    public async Task<ObservableCollection<PuzzleDTO>> GetAvailablePuzzlesAsync()
    {
        // 1. Asynchrones Laden aller DTOs aus der SQLite-Datenbank
        // Wir nutzen die bereits erstellte GetAllAsync Methode
        IEnumerable<PuzzleDTO> availablePuzzles = await _dtoSqlService.GetAllAsync<PuzzleDTO>(orderBy: "Name ASC");

        // 2. Umwandeln in eine ObservableCollection
        // Wir nutzen den Spread-Operator [..], um das IEnumerable effizient zu konvertieren
        return new ObservableCollection<PuzzleDTO>([.. availablePuzzles]);
    }
    public void SyncTimeWithModel()
    {
        CurrentPuzzle.TimeSpent = _timerService.ElapsedTime;
    }

    public void CreateNewPuzzle()
    {
        Timer.Pause();
        Timer.Reset();

        CurrentPuzzle = new Puzzle();
    }
    public async Task<bool> LoadPuzzleByIdAsync(int puzzleId)
    {
        // 1. Gezieltes Laden aus der SQLite-Datenbank
        var dto = await _dtoSqlService.GetByIdAsync<PuzzleDTO>(puzzleId);

        if (dto == null) return false;

        // 2. Mapping von DTO zu Logik-Objekt
        IPuzzle newPuzzle = DtoMapper.MapFromDto(dto);

        // Setze das aktuelle Puzzle im Service
        CurrentPuzzle = newPuzzle;

        // 3. UI-Zellen aktualisieren
        // Hinweis: Wir nutzen hier direkt die Daten aus newPuzzle
        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                // Wir aktualisieren die Digits im UI/Grid
                // (Stelle sicher, dass UpdateDigit den internen Zustand korrekt setzt)
                UpdateDigit(r, c, newPuzzle[r, c].Digit, GameType.Play);
            }
        }

        // 4. Timer initialisieren
        // Wir setzen den Timer auf den gespeicherten Stand
        Timer.Start(newPuzzle.TimeSpent);

        return true;
    }
    public async Task<bool> SaveNewCustomPuzzleAsync(bool useAutoNaming, bool alwaysAppendAutoSuffix, string defaultNamingPrefix, NamingStrategy preferredNamingStrategy)
    {
        var result = CreateAndSaveSolution(CurrentPuzzle);

        if (result)
        {

            string dtoName = CurrentPuzzle.Name ?? string.Empty;
            bool nameIsEmpty = string.IsNullOrWhiteSpace(dtoName);

            // 1. Namensgenerierung (Logik bleibt identisch)
            if (useAutoNaming && (nameIsEmpty || alwaysAppendAutoSuffix))
            {
                string baseName = nameIsEmpty ? defaultNamingPrefix : dtoName;
                CurrentPuzzle.Name = await GenerateAdvancedNameAsync(baseName, preferredNamingStrategy);
            }

            // 2. Dubletten-Check via Datenbank
            // Wir prüfen, ob der Name in der Tabelle "Puzzles" bereits existiert
            bool exists = await _dtoSqlService.ExistsAsync<PuzzleDTO>("Name", CurrentPuzzle.Name ?? string.Empty);

            if (exists)
            {
                // Hier könntest du eine Nachricht loggen oder den User informieren
                return false;
            }

            try
            {
                // 3. Speichern in der SQLite Datenbank
                int id = await _dtoSqlService.SaveAsync(DtoMapper.MapToDto(CurrentPuzzle));

                // Wichtig: Falls du die ID für spätere Updates im Spiel brauchst, 
                // sollte SaveAsync die ID zurückgeben (wie im vorigen Schritt besprochen)
                CurrentPuzzle.Id = id;

                return true;
            }
            catch (Exception)
            {
                // Fehler beim Schreiben in die DB (z.B. Dateizugriff)
                return false;
            }
        }
        return false;
    }
    public async Task SaveCurrentPuzzleProgressAsync()
    {
        if (CurrentPuzzle == null) return;

        // 1. Sicherstellen, dass die Zeit im Model aktuell ist
        SyncTimeWithModel();

        // 2. Das Puzzle in der Datenbank aktualisieren
        // Wir nutzen das Puzzle-DTO direkt, da UpdateAsync die Properties via Reflection mappt
        await _dtoSqlService.UpdateAsync(DtoMapper.MapToDto(CurrentPuzzle));
    }
    private async Task<string> GenerateAdvancedNameAsync(string prefix, NamingStrategy strategy)
    {
        if (strategy == NamingStrategy.Timestamp)
        {
            // Beispiel: "Sudoku_20260127_1045"
            return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmm}";
        }

        // Counter-Logik: Nutzt die Datenbank für den Check
        int count = 1;
        string candidate;
        bool exists;

        do
        {
            candidate = $"{prefix} {count++}";
            // Wir fragen die DB gezielt: "Gibt es diesen Namen schon?"
            exists = await _dtoSqlService.ExistsAsync<PuzzleDTO>("Name", candidate);
        }
        while (exists);

        return candidate;
    }
    public async Task DeletePuzzleAsync(int id)
    {
        // Nutzt die generische Methode, die wir vorhin im DtoSqlService gebaut haben
        await _dtoSqlService.DeleteAsync<PuzzleDTO>(id);
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
        await _dtoSqlService.UpdateAsync(mappedResetPuzzle);

        // WICHTIG: Das CurrentPuzzle im Service austauschen
        return mappedResetPuzzle;
    }

    public bool SolvePuzzle()
    {
        // Mit einer Kopie vom Puzzle Arbeiten
        Puzzle puzzleToSolve = CurrentPuzzle.Clone();
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
                    CurrentPuzzle.Solution[r, c] = puzzleToSolve[r, c].Digit;
                }
            }

            return true;
        }
        return false;
    }

    private bool CreateAndSaveSolution(IPuzzle newPuzzle)
    {
        // Mit einer Kopie vom Puzzle Arbeiten
        Puzzle puzzleToSolve = newPuzzle.Clone();
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
                    CurrentPuzzle.Solution[r, c] = puzzleToSolve[r, c].Digit;
                }
            }

            return true;
        }
        return false;
    }
    private void PrintPuzzleToDebug(IPuzzle puzzle)
    {
        if (puzzle == null) return;

        for (int r = 0; r < 9; r++)
        {
            string rowStr = "";
            for (int c = 0; c < 9; c++)
            {
                int digit = puzzle[r, c].Digit;
                rowStr += (digit == 0 ? "." : digit.ToString()) + " ";

                // Trenner für 3x3 Blöcke (vertikal)
                if ((c + 1) % 3 == 0 && c < 8) rowStr += "| ";
            }
            Debug.WriteLine(rowStr);

            // Trenner für 3x3 Blöcke (horizontal)
            if ((r + 1) % 3 == 0 && r < 8)
                Debug.WriteLine("---------------------");
        }
        Debug.WriteLine("");
    }

    public async Task<bool> VerifyAndFinishPuzzle()
    {
        bool isPuzzleCorrect = IsPuzzleCorrect();

        if (isPuzzleCorrect)
        {
            Timer.Pause();
            SyncTimeWithModel();

            CurrentPuzzle.IsSolved = true;

            await _dtoSqlService.UpdateAsync<PuzzleDTO>(DtoMapper.MapToDto(CurrentPuzzle));
            WeakReferenceMessenger.Default.Send(new PuzzleDatabaseChangedMessage());

            return true;
        }
        return false;
    }


    #region Public Update Methods
    public void UpdateColors(int colorCode)
    {
        if (!AnyCellsSelected())
            return;

        var color = (SudokuCellColor)colorCode;

        UpdateSelectedCells(cell =>
        {
            if (cell.CellColors.Any(c => c.Equals(color)))
                cell.CellColors.Remove(color);
            else
                cell.CellColors.Add(color);
        });
    }
    public void UpdateCandidates(int candidate, InputActionType selectedInputActionType)
    {
        if (candidate < 0 || candidate > IPuzzle.Size || !AnyCellsSelected())
            return;

        CandidateType type =
            selectedInputActionType == InputActionType.CenterCandidates ? CandidateType.CenterCandidates :
            selectedInputActionType == InputActionType.CornerCandidates ? CandidateType.CornerCandidates :
            throw new InvalidOperationException("No candidate modifier active");

        UpdateSelectedCells(cell =>
        {
            if (!cell.IsGiven)
                UpdateCandidate(cell.Row, cell.Column, candidate, type);
        });
    }
    public void UpdateDigits(int digit, GameType gameType = GameType.Play)
    {
        if (digit < 0 || digit > IPuzzle.Size || !AnyCellsSelected())
            return;

        UpdateSelectedCells(cell =>
        {
            if (!cell.IsGiven)
                UpdateDigit(cell.Row, cell.Column, digit, gameType);
        });
    }
    public void RemoveDigits()
    {
        UpdateSelectedCells(cell =>
        {
            if (!cell.IsGiven)
                UpdateDigit(cell.Row, cell.Column, 0);
        });
    }
    public void RemoveCandidates(CandidateType type)
    {
        UpdateSelectedCells(cell =>
        {
            if (!cell.IsGiven)
                ClearAllCandidates(type, cell.Row, cell.Column);
        });
    }
    public void RemoveColors()
    {
        UpdateSelectedCells(cell =>
        {
            cell.CellColors.Clear();
        });
    }
    public void SmartRemovalFromSelected()
    {
        var action = ResolveRemovalAction();

        if (action == RemovalAction.None)
            return;

        switch (action)
        {
            case RemovalAction.Digit:
                RemoveDigits();
                break;

            case RemovalAction.CenterCandidates:
                RemoveCandidates(CandidateType.CenterCandidates);
                break;

            case RemovalAction.CornerCandidates:
                RemoveCandidates(CandidateType.CornerCandidates);
                break;

            case RemovalAction.Colors:
                RemoveColors();
                break;
        }
    }
    #endregion

    #region Private Update Methods

    private RemovalAction ResolveRemovalAction()
    {
        bool hasDigits = false;
        bool hasCorner = false;
        bool hasCenter = false;
        bool hasColors = false;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var cell = CurrentPuzzle[r, c];

                if (!cell.IsSelected) continue;

                if (cell.Digit != 0 && !cell.IsGiven)
                    hasDigits = true;

                if (cell.CornerCandidates.Any())
                    hasCorner = true;

                if (cell.CenterCandidates.Any())
                    hasCenter = true;

                if (cell.CellColors.Any())
                    hasColors = true;
            }
        }

        if (hasDigits) return RemovalAction.Digit;
        if (hasCenter) return RemovalAction.CenterCandidates;
        if (hasCorner) return RemovalAction.CornerCandidates;
        if (hasColors) return RemovalAction.Colors;

        return RemovalAction.None;
    }
    private void UpdateSelectedCells(Action<Cell> action)
    {
        if (CurrentPuzzle == null || action == null)
            return;

        CurrentPuzzle.BeginBatchUpdate();

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var cell = CurrentPuzzle[r, c];
                if (!cell.IsSelected)
                    continue;

                action(cell);
            }
        }

        CurrentPuzzle.EndBatchUpdate();
    }
    private bool AnyCellsSelected()
    {
        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var cell = CurrentPuzzle[r, c];
                if (cell.IsSelected) return true;
            }
        }
        return false;
    }
    private void UpdateDigit(int row, int column, int digit, GameType gameType = GameType.Play)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));
        if (digit < 0 || digit > 9) throw new ArgumentOutOfRangeException(nameof(digit), "Digit must be between 0 and 9.");

        var currentDigit = CurrentPuzzle[row, column].Digit;

        if (currentDigit == digit)
        {
            CurrentPuzzle[row, column].Digit = 0;
            if (gameType == GameType.Create) CurrentPuzzle[row, column].IsGiven = false;
            CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.CenterCandidates, row, column, currentDigit);
            CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.CornerCandidates, row, column, currentDigit);
            CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.SolverCandidates, row, column, currentDigit);
        }
        else
        {
            if (digit == 0)
            {
                CurrentPuzzle[row, column].Digit = 0;
                if (gameType == GameType.Create) CurrentPuzzle[row, column].IsGiven = false;
                CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.CenterCandidates, row, column, currentDigit);
                CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.CornerCandidates, row, column, currentDigit);
                CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.SolverCandidates, row, column, currentDigit);
            }
            else
            {
                var oldDigit = CurrentPuzzle[row, column].Digit;

                CurrentPuzzle[row, column].Digit = digit;
                if (gameType == GameType.Create) CurrentPuzzle[row, column].IsGiven = true;

                // Clear all candidates in the current cell
                CandidateManager.ClearAllCandidatesInCell(CurrentPuzzle, row, column);
                RemoveSolverCandidatesInRelatedUnits(row, column, digit);
                RemoveCenterCandidatesInRelatedUnits(row, column, digit);
                RemoveCornerCandidatesInRelatedUnits(row, column, digit);

                CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.CenterCandidates, row, column, currentDigit);
                CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.CornerCandidates, row, column, currentDigit);
                CandidateManager.RestoreCandidates(CurrentPuzzle, CandidateType.SolverCandidates, row, column, currentDigit);

            }
        }
    }
    private void UpdateCandidate(int row, int column, int digit, CandidateType type)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));

        switch (type)
        {
            case CandidateType.CenterCandidates:
                if (CurrentPuzzle[row, column].CenterCandidates.Contains(digit))
                    CurrentPuzzle[row, column].CenterCandidates[digit] = false;
                else
                    CurrentPuzzle[row, column].CenterCandidates[digit] = true;
                break;

            case CandidateType.CornerCandidates:
                if (CurrentPuzzle[row, column].CornerCandidates.Contains(digit))
                    CurrentPuzzle[row, column].CornerCandidates[digit] = false;
                else
                    CurrentPuzzle[row, column].CornerCandidates[digit] = true;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Unknown CandidateType");
        }
    }
    private void ClearAllCandidates(CandidateType candidateType, int row, int column)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));

        switch (candidateType)
        {
            case CandidateType.CenterCandidates:
                CurrentPuzzle[row, column].CenterCandidates.Clear();
                break;
            case CandidateType.CornerCandidates:
                CurrentPuzzle[row, column].CornerCandidates.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(candidateType), "Unknown CandidateType");
        }
    }
    private void RemoveSolverCandidatesInRelatedUnits(int row, int column, int digit)
    {
        int boxRow = row / 3;
        int boxCol = column / 3;
        int boxIndex = boxRow * 3 + boxCol;
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Row, CandidateType.SolverCandidates, row, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Column, CandidateType.SolverCandidates, column, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Box, CandidateType.SolverCandidates, boxIndex, (row, column), digit);
    }
    private void RemoveCenterCandidatesInRelatedUnits(int row, int column, int digit)
    {
        int boxRow = row / 3;
        int boxCol = column / 3;
        int boxIndex = boxRow * 3 + boxCol;
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Row, CandidateType.CenterCandidates, row, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Column, CandidateType.CenterCandidates, column, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Box, CandidateType.CenterCandidates, boxIndex, (row, column), digit);
    }
    private void RemoveCornerCandidatesInRelatedUnits(int row, int column, int digit)
    {
        int boxRow = row / 3;
        int boxCol = column / 3;
        int boxIndex = boxRow * 3 + boxCol;
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Row, CandidateType.CornerCandidates, row, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Column, CandidateType.CornerCandidates, column, (row, column), digit);
        CandidateManager.RemoveCandidatesInUnit(CurrentPuzzle, UnitType.Box, CandidateType.CornerCandidates, boxIndex, (row, column), digit);
    }
    #endregion

    #region Validation
    public bool IsValidDigit(int row, int column, int digit)
    {
        if ((uint)row >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)column >= IPuzzle.Size) throw new ArgumentOutOfRangeException(nameof(column));
        if (digit < 1 || digit > 9) throw new ArgumentOutOfRangeException(nameof(digit), "Digit must be between 1 and 9.");
        // Check the row
        for (int c = 0; c < IPuzzle.Size; c++)
        {
            if (c != column && CurrentPuzzle[row, c].Digit == digit)
                return false;
        }
        // Check the column
        for (int r = 0; r < IPuzzle.Size; r++)
        {
            if (r != row && CurrentPuzzle[r, column].Digit == digit)
                return false;
        }
        // Check the 3x3 box
        int boxStartRow = (row / 3) * 3;
        int boxStartCol = (column / 3) * 3;
        for (int r = boxStartRow; r < boxStartRow + 3; r++)
        {
            for (int c = boxStartCol; c < boxStartCol + 3; c++)
            {
                if ((r != row || c != column) && CurrentPuzzle[r, c].Digit == digit)
                    return false;
            }
        }
        return true;
    }
    private bool IsPuzzleCorrect()
    {
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
