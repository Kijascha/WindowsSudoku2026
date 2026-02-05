using Dapper;
using Microsoft.Data.Sqlite;
using System.Configuration;
using System.IO;
using WindowsSudoku2026.Core.Interfaces;

namespace WindowsSudoku2026.Infrastructure.Services;

public class SQLiteService : IDisposable, ISQLiteService
{
    private readonly string _connectionString;
    protected SqliteConnection? _connection;

    public SQLiteService()
    {
        // ConnectionString aus der Config laden
        var settings = ConfigurationManager.ConnectionStrings["SudokuDb"];
        if (settings == null)
            throw new InvalidOperationException("ConnectionString 'SudokuDb' nicht in App.config gefunden!");

        _connectionString = settings.ConnectionString;
        // Datenbank-Datei und Pfad sicherstellen, falls sie noch nicht existieren
        EnsureDatabaseDirectoryExists();
    }
    // Lazy Loading der Verbindung: Sie wird erst geöffnet, wenn sie gebraucht wird
    private SqliteConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
        }
        return _connection;
    }

    private void EnsureDatabaseDirectoryExists()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionString);
        string? directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        return await GetConnection().ExecuteAsync(sql, param);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        return await GetConnection().QueryAsync<T>(sql, param);
    }
    // Hilfreich für EXISTS-Abfragen
    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        return await GetConnection().ExecuteScalarAsync<T>(sql, param);
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}
