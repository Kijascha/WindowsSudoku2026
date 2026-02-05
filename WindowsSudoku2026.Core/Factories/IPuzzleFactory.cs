using WindowsSudoku2026.Common.Models;

namespace WindowsSudoku2026.Core.Factories;

public interface IPuzzleFactory<TPuzzle> where TPuzzle : IPuzzle
{
    TPuzzle Create();
}