using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.Core.Factories;

public class AbstractFactory<TViewModel>(Func<TViewModel> factory) : IAbstractFactory<TViewModel> where TViewModel : IViewModel
{
    private readonly Func<TViewModel> _factory = factory;
    public TViewModel Create() => _factory();
}
