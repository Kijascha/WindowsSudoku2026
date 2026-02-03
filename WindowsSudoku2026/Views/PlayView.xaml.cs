using System.Windows.Controls;
using System.Windows.Input;
using WindowsSudoku2026.ViewModels;

namespace WindowsSudoku2026.Views
{
    /// <summary>
    /// Interaktionslogik für PlayView.xaml
    /// </summary>
    public partial class PlayView : UserControl
    {
        public PlayView()
        {
            InitializeComponent();
        }
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            var viewModel = DataContext as PlayViewModel;
            if (viewModel != null)
                viewModel.ActiveModifiers = Keyboard.Modifiers;
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
            var viewModel = DataContext as PlayViewModel;
            if (viewModel != null)
                viewModel.ActiveModifiers = Keyboard.Modifiers;
        }
    }
}
