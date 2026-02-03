namespace WindowsSudoku2026.Services
{
    public interface IDtoSqlService
    {
        Task DeleteAsync<T>(int id) where T : class;
        Task ExecuteGenericAsync<T>(string sql, T dto) where T : class;
        Task<bool> ExistsAsync<T>(string fieldName, object value) where T : class;
        Task<IEnumerable<T>> GetAllAsync<T>(string orderBy = "Id") where T : class;
        Task<int> SaveAsync<T>(T dto) where T : class;
        Task<T?> GetByIdAsync<T>(int id) where T : class;
        Task UpdateAsync<T>(T dto) where T : class;
        Task UpdatePartialAsync<T>(int id, object updateFields) where T : class;
    }
}