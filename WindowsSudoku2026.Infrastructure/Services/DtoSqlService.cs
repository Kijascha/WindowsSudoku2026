using Dapper;
using System.Reflection;
using System.Text;
using WindowsSudoku2026.Common.Attributes;
using WindowsSudoku2026.Core.Interfaces;

namespace WindowsSudoku2026.Infrastructure.Services;

public class DtoSqlService(ISQLiteService sqlite) : IDtoSqlService
{
    // Hilfsmethode, um den Tabellennamen aus dem Attribut zu lesen
    private static string GetTableName<T>() =>
        typeof(T).GetCustomAttributes(typeof(TableAttribute), true)
                 .FirstOrDefault() is TableAttribute attr ? attr.Name : typeof(T).Name;

    /// <summary>
    /// Speichert ein beliebiges DTO-Objekt in der entsprechenden Datenbanktabelle.
    /// Die "Id" wird ignoriert, da sie von SQLite (Auto-Increment) vergeben wird.
    /// </summary>
    public async Task<int> SaveAsync<T>(T dto) where T : class
    {
        var type = typeof(T);
        var tableName = GetTableName<T>();

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.Name != "Id")
                             .ToList();

        var columnNames = string.Join(", ", properties.Select(p => p.Name));
        var parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));

        var sb = new StringBuilder();
        sb.Append($"INSERT INTO {tableName} ");
        sb.Append($"({columnNames}) ");
        sb.Append($"VALUES ({parameterNames});"); // Wichtig: Semikolon hier beibehalten

        // --- KORREKTURSTART ---

        // Wir fügen den Befehl zum Abfragen der neuen ID HIER direkt an
        sb.Append(" SELECT last_insert_rowid();");
        string sql = sb.ToString();

        // Wir nutzen ExecuteScalarAsync, um das KOMPLETTE Statement auszuführen
        // und die generierte ID zu erhalten. Das Statement wird nur einmal ausgeführt.
        var newId = await sqlite.ExecuteScalarAsync<int>(sql, dto);

        // --- KORREKTURENDE ---

        // Optional: Per Reflection die ID im Objekt setzen
        typeof(T).GetProperty("Id")?.SetValue(dto, newId);

        return newId;
    }

    /// <summary>
    /// Holt alle Einträge einer Tabelle, optional sortiert.
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync<T>(string orderBy = "Id") where T : class
    {
        string sql = $"SELECT * FROM {GetTableName<T>()} ORDER BY {orderBy}";
        return await sqlite.QueryAsync<T>(sql);
    }

    /// <summary>
    /// Löscht einen Eintrag anhand der ID aus der jeweiligen Tabelle.
    /// </summary>
    public async Task DeleteAsync<T>(int id) where T : class
    {
        string sql = $"DELETE FROM {GetTableName<T>()} WHERE Id = @Id";
        await sqlite.ExecuteAsync(sql, new { Id = id });
    }

    /// <summary>
    /// Prüft die Existenz anhand eines beliebigen Feldes (z.B. Name).
    /// </summary>
    public async Task<bool> ExistsAsync<T>(string fieldName, object value) where T : class
    {
        string sql = $"SELECT COUNT(1) FROM {GetTableName<T>()} WHERE {fieldName} = @Value";
        var count = await sqlite.ExecuteScalarAsync<int>(sql, new { Value = value });
        return count > 0;
    }
    /// <summary>
    /// Aktualisiert ein komplettes DTO in der Datenbank.
    /// Nutzt Reflection, um alle Spalten außer 'Id' automatisch zu mappen.
    /// </summary>
    public async Task UpdateAsync<T>(T dto) where T : class
    {
        var type = typeof(T);
        var tableName = GetTableName<T>();

        // Alle öffentlichen Instanz-Properties holen
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.Name != "Id") // ID ist unser Schlüssel, kein SET-Feld
                             .ToList();

        // Erzeugt: Name = @Name, Digits = @Digits, ...
        var setSql = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

        var sql = $"UPDATE {tableName} SET {setSql} WHERE Id = @Id";

        // Dapper mappt das gesamte DTO-Objekt gegen die @Parameter
        await sqlite.ExecuteAsync(sql, dto);
    }
    public async Task UpdatePartialAsync<T>(int id, object updateFields) where T : class
    {
        var tableName = GetTableName<T>();
        // Holt die Namen der Properties aus dem anonymen Objekt (z.B. TimeSpentTicks)
        var props = updateFields.GetType().GetProperties().Select(p => p.Name);

        var setSql = string.Join(", ", props.Select(p => $"{p} = @{p}"));
        var sql = $"UPDATE {tableName} SET {setSql} WHERE Id = @Id";

        // Wir mischen die ID unter die Parameter
        var parameters = new DynamicParameters(updateFields);
        parameters.Add("Id", id);

        await sqlite.ExecuteAsync(sql, parameters);
    }
    public async Task<T?> GetByIdAsync<T>(int id) where T : class
    {
        string sql = $"SELECT * FROM {GetTableName<T>()} WHERE Id = @Id";
        var result = await sqlite.QueryAsync<T>(sql, new { Id = id });
        return result.FirstOrDefault();
    }
    /// <summary>
    /// Da INSERTs und UPDATEs spezifische Spalten brauchen, 
    /// übergeben wir hier das fertige SQL-Statement vom spezialisierten Provider/Mapper.
    /// </summary>
    public async Task ExecuteGenericAsync<T>(string sql, T dto) where T : class
    {
        await sqlite.ExecuteAsync(sql, dto);
    }
}
