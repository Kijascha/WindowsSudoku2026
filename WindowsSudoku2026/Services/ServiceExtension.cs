using Microsoft.Extensions.DependencyInjection;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Core.Factories;
using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.Services;

public static class ServiceExtension
{
    extension(IServiceCollection services)
    {
        public void AddAbstractFactory<TViewModel>() where TViewModel : ViewModel
        {
            services.AddTransient<TViewModel>();
            services.AddSingleton<Func<TViewModel>>((p) => () => p.GetRequiredService<TViewModel>());
            services.AddSingleton<IAbstractFactory<TViewModel>, AbstractFactory<TViewModel>>();
        }
        public void AddPuzzleFactory<TPuzzle>() where TPuzzle : Puzzle
        {
            services.AddTransient<IPuzzle, TPuzzle>();
            services.AddSingleton<Func<TPuzzle>>((p) => () => p.GetRequiredService<TPuzzle>());
            services.AddSingleton<IPuzzleFactory<TPuzzle>, PuzzleFactory<TPuzzle>>();
        }
    }
    extension(IServiceProvider provider)
    {
        public IAbstractFactory<TViewModel> GetAbstractFactory<TViewModel>()
        where TViewModel : IViewModel
        {
            return provider.GetRequiredService<IAbstractFactory<TViewModel>>();
        }
        public IPuzzleFactory<TPuzzle> GetPuzzleFactory<TPuzzle>()
        where TPuzzle : IPuzzle
        {
            return provider.GetRequiredService<IPuzzleFactory<TPuzzle>>();
        }
    }
}
