using System.Windows;
using System.Windows.Controls;

namespace WindowsSudoku2026.Views
{
    /// <summary>
    /// Interaktionslogik für ChoseCustomPuzzleView.xaml
    /// </summary>
    public partial class ChoseCustomPuzzleView : UserControl
    {
        public ChoseCustomPuzzleView()
        {
            InitializeComponent();
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
