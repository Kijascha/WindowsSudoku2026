using WindowsSudoku2026.Core.Interfaces;

namespace WindowsSudoku2026.Infrastructure.Services;

public class DtoJsonService(IJsonService jsonService) : IDtoJsonService
{
    public bool SaveDto<TDto>(string filePath, TDto data, Func<List<TDto>, TDto, bool> validationFunc) where TDto : class
    {
        // 1. Laden oder neue Liste erstellen, falls Datei fehlt
        var existingData = jsonService.LoadAll<List<TDto>>(filePath) ?? new List<TDto>();

        // 2. Validierung via Delegate
        // Die validationFunc sollte 'true' zurückgeben, wenn alles okay ist
        if (!validationFunc(existingData, data))
            return false;

        // 3. Hinzufügen und Speichern
        existingData.Add(data);
        jsonService.SaveAll(filePath, existingData);

        return true;
    }
    public List<TDto> GetAllDtos<TDto>(string filePath)
    {
        // Wenn die Datei nicht existiert, geben wir eine leere Liste zurück, 
        // damit der Aufrufer (z.B. UI) direkt damit arbeiten kann (kein null-Check nötig).
        return jsonService.LoadAll<List<TDto>>(filePath) ?? new List<TDto>();
    }
    public TDto? GetDto<TDto>(string filePath, Func<TDto, bool> predicate)
    {
        var allData = GetAllDtos<TDto>(filePath);

        // Nutzt Linq, um das erste Element zu finden, das die Bedingung erfüllt
        return allData.FirstOrDefault(predicate);
    }
    public bool UpdateDto<TDto>(string filePath, TDto data, Func<TDto, bool> findPredicate) where TDto : class
    {
        var existingData = jsonService.LoadAll<List<TDto>>(filePath) ?? new List<TDto>();

        // 1. Den Index des vorhandenen Eintrags finden
        int index = existingData.FindIndex(new Predicate<TDto>(findPredicate));

        if (index == -1) return false; // Nicht gefunden -> Update nicht möglich

        // 2. Den alten Eintrag durch den neuen (aktuellen State) ersetzen
        existingData[index] = data;

        // 3. Die aktualisierte Liste speichern
        jsonService.SaveAll(filePath, existingData);

        return true;
    }
    public bool SaveDtoInSettings<TContainer, TDto>(
    string filePath,
    string sectionName,
    TDto data,
    Func<List<TDto>, TDto, bool> validationFunc)
    where TContainer : class, IHasDtoList<TDto>, new() // <-- Interface hier erzwingen!
    where TDto : class
    {
        // 1. Gesamte Datei als Dictionary laden
        var fileContent = jsonService.LoadAll<Dictionary<string, TContainer>>(filePath)
                          ?? new Dictionary<string, TContainer>();

        if (!fileContent.TryGetValue(sectionName, out var container))
        {
            container = new TContainer();
        }

        // 2. Zugriff über das Interface (jetzt sicher durch Constraint)
        var list = container.Items;

        // 3. Validierung & Hinzufügen
        if (!validationFunc(list, data)) return false;
        list.Add(data);

        // 4. Speichern
        fileContent[sectionName] = container;
        jsonService.SaveAll(filePath, fileContent);

        return true;
    }
    public bool UpdateDtoInSettings<TContainer, TDto>(
    string filePath,
    string sectionName,
    TDto data,
    Func<TDto, bool> findPredicate)
    where TContainer : class, IHasDtoList<TDto>, new()
    where TDto : class
    {
        // 1. Gesamte Datei als Dictionary laden, um den Root-Knoten zu erhalten
        var fileContent = jsonService.LoadAll<Dictionary<string, TContainer>>(filePath)
                          ?? new Dictionary<string, TContainer>();

        // 2. Die Sektion (z.B. "ColorPaletteSettings") suchen
        if (!fileContent.TryGetValue(sectionName, out var container))
            return false;

        // 3. Über das Interface 'IHasDtoList' auf die 'Items' (Palettes) zugreifen
        var list = container.Items;
        int index = list.FindIndex(new Predicate<TDto>(findPredicate));

        if (index == -1) return false;

        // 4. Den Eintrag in der Liste ersetzen
        list[index] = data;

        // 5. Die gesamte Struktur (inkl. ActivePaletteId) zurückschreiben
        fileContent[sectionName] = container;
        jsonService.SaveAll(filePath, fileContent);

        return true;
    }
    public bool DeleteDtoFromSettings<TContainer, TDto>(
    string filePath,
    string sectionName,
    Func<TDto, bool> findPredicate)
    where TContainer : class, IHasDtoList<TDto>, new()
    where TDto : class
    {
        // 1. Gesamte Datei laden
        var fileContent = jsonService.LoadAll<Dictionary<string, TContainer>>(filePath)
                          ?? new Dictionary<string, TContainer>();

        // 2. Sektion suchen
        if (!fileContent.TryGetValue(sectionName, out var container))
            return false;

        // 3. Index des zu löschenden Elements finden
        var list = container.Items;
        int index = list.FindIndex(new Predicate<TDto>(findPredicate));

        if (index == -1) return false;

        // Speichern des Objekts für die Benachrichtigung, bevor es gelöscht wird
        var deletedItem = list[index];

        // 4. Den Eintrag aus der Liste entfernen
        list.RemoveAt(index);

        // 5. Struktur aktualisieren und speichern
        fileContent[sectionName] = container;
        jsonService.SaveAll(filePath, fileContent);

        return true;
    }
}
