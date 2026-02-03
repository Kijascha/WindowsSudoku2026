using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using WindowsSudoku2026.Messaging;

namespace WindowsSudoku2026.Services;

public abstract class JsonServiceBase
{
    protected readonly IJsonService _jsonService;
    protected readonly IConfiguration _configuration;

    protected JsonServiceBase(IJsonService jsonService, IConfiguration configuration)
    {
        _jsonService = jsonService;
        _configuration = configuration;
    }

    /// <summary>
    /// Erzwingt den Reload der .NET Konfiguration und benachrichtigt die App.
    /// </summary>
    protected void NotifyChange<T>(T data) where T : class
    {
        if (_configuration is IConfigurationRoot root)
        {
            root.Reload();
        }

        // Wir senden die generische Nachricht an alle ViewModels
        WeakReferenceMessenger.Default.Send(new GenericSettingsChangedMessage<T>(data));
    }
}
