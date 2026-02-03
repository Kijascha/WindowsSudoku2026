using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace WindowsSudoku2026.Core.ViewModels;

public class ViewModel() : ObservableObject, IDisposable, IViewModel
{
    private bool _disposedValue;

    /// <summary>
    /// Implementierung von IDisposable.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Zentrale Methode zum Aufräumen. 
    /// Kann in abgeleiteten ViewModels mit 'override' erweitert werden.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // 1. Automatisch vom Messenger abmelden
                WeakReferenceMessenger.Default.UnregisterAll(this);

                // 2. Hier können weitere verwaltete Ressourcen (Events, Timer) 
                // in den Unterklassen bereinigt werden.
            }

            _disposedValue = true;
        }
    }
}
