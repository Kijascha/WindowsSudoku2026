using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using WindowsSudoku2026.ViewModels;

namespace WindowsSudoku2026.Views
{
    /// <summary>
    /// Interaktionslogik für PuzzleSelectionView.xaml
    /// </summary>
    public partial class PuzzleSelectionView : UserControl
    {
        public PuzzleSelectionView()
        {
            InitializeComponent();
            DataContext = App.AppHost?.Services.GetRequiredService<PuzzleSelectionViewModel>();
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RootGrid.ActualWidth > 750)
                SelectionListBox.Width = 700;
            else
                SelectionListBox.Width = 450;
        }
    }
}
