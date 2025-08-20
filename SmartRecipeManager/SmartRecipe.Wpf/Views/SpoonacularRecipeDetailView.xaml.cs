using SmartRecipe.Wpf.ViewModels;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SpoonacularRecipeDetailView.xaml
    /// </summary>
    public partial class SpoonacularRecipeDetailView : UserControl
    {
        public SpoonacularRecipeDetailView(SpoonacularRecipeDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}