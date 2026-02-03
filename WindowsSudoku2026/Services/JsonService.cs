using Newtonsoft.Json;
using System.IO;

namespace WindowsSudoku2026.Services;

public class JsonService : IJsonService
{
    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        // Verhindert Probleme, wenn Klassenstrukturen sich leicht ändern
        MissingMemberHandling = MissingMemberHandling.Ignore
    };
    public void SaveAll<TData>(string filePath, TData data)
    {
        // Erstellt alle Ordner im Pfad, falls sie noch nicht existieren
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        // Die Serialisierung erfolgt immer
        string jsonOutput = JsonConvert.SerializeObject(data, _settings);

        // File.WriteAllText erstellt die Datei im bin-Ordner, falls sie fehlt,
        // oder überschreibt sie, falls sie schon da ist.
        File.WriteAllText(filePath, jsonOutput);
    }
    public async Task SaveAllAsync<TData>(string filePath, TData data)
    {
        await Task.Run(() => SaveAll(filePath, data));
    }

    public TData? LoadAll<TData>(string filePath)
    {
        if (!File.Exists(filePath)) return default;

        try
        {
            // 2. Datei einlesen
            string jsonInput = File.ReadAllText(filePath);

            // 3. Deserialisieren
            return JsonConvert.DeserializeObject<TData>(jsonInput);
        }
        catch (Exception)
        {
            // Falls die Datei korrupt ist oder das Format nicht passt
            // Hier könnte man Logging einbauen (z.B. Debug.WriteLine)
            return default;
        }
    }

    public async Task<TData?> LoadAllAsync<TData>(string filePath)
    {
        return await Task.Run(() => LoadAll<TData>(filePath));
    }
}
