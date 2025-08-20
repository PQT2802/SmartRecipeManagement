using SmartRecipe.Wpf.ViewModels;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    public partial class RecipeListView : UserControl
    {
        public RecipeListView(RecipeListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}