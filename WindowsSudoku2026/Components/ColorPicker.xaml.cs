using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WindowsSudoku2026.ViewModels;

namespace WindowsSudoku2026.Components
{
    /// <summary>
    /// Interaktionslogik für ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
            DataContext = App.AppHost?.Services.GetRequiredService<ColorPickerViewModel>();
        }
    }
}
