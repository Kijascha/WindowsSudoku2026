using Microsoft.Extensions.DependencyInjection;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Infrastructure.Repositories;

namespace WindowsSudoku2026.Infrastructure.Services;

public static class ServiceExtension
{
    extension(IServiceCollection services)
    {
        public void AddInfrastucture()
        {
            services.AddSingleton<IJsonService, JsonService>();
            services.AddSingleton<ISQLiteService, SQLiteService>();
            services.AddSingleton<IDtoJsonService, DtoJsonService>();
            services.AddSingleton<IDtoSqlService, DtoSqlService>();

            services.AddSingleton<ISudokuRepository, SudokuRepository>();
            services.AddSingleton<IColorPaletteRepository, ColorPaletteRepository>();

            services.AddSingleton<ISettingsService, SettingsService>();
        }
    }
}
