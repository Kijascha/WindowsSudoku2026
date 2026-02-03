using Microsoft.Extensions.DependencyInjection;
using WindowsSudoku2026.Core.Factories;
using WindowsSudoku2026.Core.ViewModels;
using WindowsSudoku2026.ViewModels;

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
        public void AddAbstractValidatorFactory<TViewModel>() where TViewModel : ViewModelValidator
        {
            services.AddTransient<TViewModel>();
            services.AddSingleton<Func<TViewModel>>((p) => () => p.GetRequiredService<TViewModel>());
            services.AddSingleton<IAbstractFactory<TViewModel>, AbstractFactory<TViewModel>>();
        }
    }
    extension(IServiceProvider provider)
    {
        public IAbstractFactory<TViewModel> GetAbstractFactory<TViewModel>()
        where TViewModel : IViewModel
        {
            return provider.GetRequiredService<IAbstractFactory<TViewModel>>();
        }
    }
}
