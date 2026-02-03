using WindowsSudoku2026.Common.DTO;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Utils.Colors;

namespace WindowsSudoku2026.DTO;

public static class DtoMapper
{
    public static ColorPaletteDTOV2 MapToDto(ColorPalette palette)
    {
        return new()
        {
            Id = palette.Id,
            ColorPaletteString = DtoDataConverter.EncodePalette(palette.SudokuColors)
        };
    }
    public static ColorPalette MapFromDto(ColorPaletteDTOV2 dto)
    {
        return new([.. DtoDataConverter.DecodePalette(dto.ColorPaletteString)])
        {
            Id = dto.Id
        };
    }
    public static PuzzleDTO MapToDto(IPuzzle puzzle)
    {
        return new PuzzleDTO()
        {
            Id = puzzle.Id,
            Name = puzzle.Name ?? string.Empty,
            PreviewImage = DtoDataConverter.EncodeBitmapSourceToBytes(puzzle.PreviewImage),
            Digits = DtoDataConverter.EncodeDigits(puzzle),
            SolutionString = DtoDataConverter.EncodeSolution(puzzle),
            CenterCandidateBitmasks = DtoDataConverter.EncodeBitmasks(puzzle, c => c.CenterCandidates),
            CornerCandidateBitmasks = DtoDataConverter.EncodeBitmasks(puzzle, c => c.CornerCandidates),
            SolverCandidateBitmasks = DtoDataConverter.EncodeBitmasks(puzzle, c => c.SolverCandidates),
            IsGivenString = DtoDataConverter.EncodeIsGiven(puzzle),
            CellColors = DtoDataConverter.EncodeCellColors(puzzle),
            TimeSpentTicks = puzzle.TimeSpent.Ticks,
            IsSolved = puzzle.IsSolved
        };
    }

    public static IPuzzle MapFromDto(PuzzleDTO dto)
    {
        var newPuzzle = new Puzzle(dto.Id)
        {
            Name = dto.Name,
            PreviewImage = DtoDataConverter.DecodeBitmapSourceFromBytes(dto.PreviewImage),
            TimeSpent = TimeSpan.FromTicks(dto.TimeSpentTicks),
            IsSolved = dto.IsSolved,
        };

        bool okDigits = DtoDataConverter.TryDecodeDigits(newPuzzle, dto.Digits);
        bool okSolution = DtoDataConverter.TryDecodeSolution(newPuzzle, dto.SolutionString);
        bool okCenter = DtoDataConverter.TryDecodeBitmasks(newPuzzle, dto.CenterCandidateBitmasks, (a, b) => a.CenterCandidates = b);
        bool okCorner = DtoDataConverter.TryDecodeBitmasks(newPuzzle, dto.CornerCandidateBitmasks, (a, b) => a.CornerCandidates = b);
        bool okSolver = DtoDataConverter.TryDecodeBitmasks(newPuzzle, dto.SolverCandidateBitmasks, (a, b) => a.SolverCandidates = b);
        bool okGiven = string.IsNullOrEmpty(dto.IsGivenString) || DtoDataConverter.TryDecodeIsGiven(newPuzzle, dto.IsGivenString);
        bool okColors = DtoDataConverter.TryDecodeCellColors(newPuzzle, dto.CellColors);

        bool allOk = okDigits && okSolution && okGiven && okCenter && okCorner && okSolver && okColors;
        return allOk ? newPuzzle : new Puzzle();
    }
}
