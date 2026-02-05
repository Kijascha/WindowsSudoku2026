namespace WindowsSudoku2026.Core.Interfaces;

public interface IJsonService
{
    TData? LoadAll<TData>(string filePath);
    Task<TData?> LoadAllAsync<TData>(string filePath);
    void SaveAll<TData>(string filePath, TData data);
    Task SaveAllAsync<TData>(string filePath, TData data);
}
