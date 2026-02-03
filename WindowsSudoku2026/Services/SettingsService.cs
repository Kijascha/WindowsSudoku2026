using Microsoft.Extensions.Configuration;

namespace WindowsSudoku2026.Services;

public class SettingsService(IJsonService jsonService, IConfiguration configuration) : JsonServiceBase(jsonService, configuration), ISettingsService
{
    /// <summary>
    /// Speichert ein komplettes Settings-Objekt (überschreibt die Sektion).
    /// </summary>
    public void SaveSettings<T>(string filePath, string sectionName, T settings) where T : class
    {
        // 1. Aktuelle Datei laden (Read-Modify-Write), um andere Sektionen nicht zu löschen
        // Wir nutzen ein Dictionary, um flexibel auf den sectionName zuzugreifen
        var fileContent = _jsonService.LoadAll<Dictionary<string, object>>(filePath)
                          ?? new Dictionary<string, object>();

        // 2. Die spezifische Sektion aktualisieren oder hinzufügen
        fileContent[sectionName] = settings;

        // 3. Zurückschreiben
        _jsonService.SaveAll(filePath, fileContent);

        // 4. System benachrichtigen
        //NotifyChange(settings);
    }

    /// <summary>
    /// Lädt die existierenden Settings, führt eine Änderung aus und speichert nur diese Sektion zurück.
    /// Ideal für Teil-Updates (z.B. nur die ActivePaletteId ändern).
    /// </summary>
    public void UpdateSettings<T>(string filePath, string sectionName, Action<T> updateAction) where T : class, new()
    {
        // 1. Datei laden
        var fileContent = _jsonService.LoadAll<Dictionary<string, object>>(filePath)
                          ?? new Dictionary<string, object>();

        T settings;

        // 2. Prüfen, ob die Sektion existiert, sonst neue Instanz
        if (fileContent.TryGetValue(sectionName, out var existingSection))
        {
            // Da jsonService Dictionary<string, object> liefert, müssen wir das object ggf. umwandeln
            // Newtonsoft/System.Text.Json speichert Objekte im Dictionary oft als JObject/JsonElement
            var json = existingSection.ToString();
            settings = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json!) ?? new T();
        }
        else
        {
            settings = new T();
        }

        // 3. Die vom Aufrufer definierte Änderung anwenden
        updateAction(settings);

        // 4. In das Dictionary zurückschreiben und Datei speichern
        fileContent[sectionName] = settings;
        _jsonService.SaveAll(filePath, fileContent);

        // 5. Systemweit benachrichtigen (Reload & Messenger)
        //NotifyChange(settings);
    }
}
