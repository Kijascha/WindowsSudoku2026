namespace WindowsSudoku2026.Services
{
    public interface ISQLiteService
    {
        void Dispose();
        Task<int> ExecuteAsync(string sql, object? param = null);
        Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
    }
}