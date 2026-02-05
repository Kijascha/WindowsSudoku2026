using Microsoft.Extensions.DependencyInjection;
using WindowsSudoku2026.Core.Interfaces;

namespace WindowsSudoku2026.Core.Services
{
    public static class ServiceExtension
    {
        extension(IServiceCollection services)
        {
            public void AddCoreServices()
            {
                services.AddSingleton<IGameServiceV2, GameServiceV2>();
                services.AddSingleton<IPuzzleCommandService, PuzzleCommandService>();
                services.AddTransient<IPuzzleManagerService, PuzzleManagerService>();
            }
        }
    }
}
