using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WindowsSudoku2026.ViewModels;

namespace WindowsSudoku2026.Components
{
    /// <summary>
    /// Interaktionslogik für ColorPaletteSelection.xaml
    /// </summary>
    public partial class ColorPaletteSelection : UserControl
    {
        public ColorPaletteSelection()
        {
            InitializeComponent();
            DataContext = App.AppHost?.Services.GetRequiredService<ColorPaletteSelectionViewModel>();
        }
    }
}
