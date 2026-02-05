namespace WindowsSudoku2026.Core.Interfaces;

public interface ISettingsService
{
    void SaveSettings<T>(string filePath, string sectionName, T settings) where T : class;
    void UpdateSettings<T>(string filePath, string sectionName, Action<T> updateAction) where T : class, new();
}
