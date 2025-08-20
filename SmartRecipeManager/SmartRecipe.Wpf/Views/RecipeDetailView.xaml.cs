using SmartRecipe.Wpf.ViewModels;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    public partial class RecipeDetailView : UserControl
    {
        public RecipeDetailView(RecipeDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}