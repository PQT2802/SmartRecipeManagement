using SmartRecipe.Wpf.ViewModels;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView(DashboardViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}