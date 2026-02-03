using WindowsSudoku2026.Essential;

namespace WindowsSudoku2026.Services;

public interface IDtoJsonService
{
    List<TDto> GetAllDtos<TDto>(string filePath);
    TDto? GetDto<TDto>(string filePath, Func<TDto, bool> predicate);
    bool SaveDto<TDto>(string filePath, TDto data, Func<List<TDto>, TDto, bool> validationFunc) where TDto : class;
    bool UpdateDto<TDto>(string filePath, TDto data, Func<TDto, bool> findPredicate) where TDto : class;
    public bool SaveDtoInSettings<TContainer, TDto>(
    string filePath,
    string sectionName,
    TDto data,
    Func<List<TDto>, TDto, bool> validationFunc)
    where TContainer : class, IHasDtoList<TDto>, new() // <-- Interface hier erzwingen!
    where TDto : class;
    public bool UpdateDtoInSettings<TContainer, TDto>(
    string filePath,
    string sectionName,
    TDto data,
    Func<TDto, bool> findPredicate)
    where TContainer : class, IHasDtoList<TDto>, new()
    where TDto : class;
    public bool DeleteDtoFromSettings<TContainer, TDto>(
    string filePath,
    string sectionName,
    Func<TDto, bool> findPredicate)
    where TContainer : class, IHasDtoList<TDto>, new()
    where TDto : class;
}