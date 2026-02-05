using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Core.Factories;

public class PuzzleFactory<TPuzzle>(Func<TPuzzle> factory) : IPuzzleFactory<TPuzzle> where TPuzzle : IPuzzle
{
    private readonly Func<TPuzzle> _factory = factory;
    public TPuzzle Create() => _factory();
}
