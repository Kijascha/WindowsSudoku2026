namespace WindowsSudoku2026.Core.Interfaces;

public interface IHasDtoList<TDto>
{
    List<TDto> Items { get; }
}
