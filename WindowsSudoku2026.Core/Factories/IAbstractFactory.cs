using WindowsSudoku2026.Core.ViewModels;

namespace WindowsSudoku2026.Core.Factories
{
    public interface IAbstractFactory<TViewModel> where TViewModel : IViewModel
    {
        TViewModel Create();
    }
}