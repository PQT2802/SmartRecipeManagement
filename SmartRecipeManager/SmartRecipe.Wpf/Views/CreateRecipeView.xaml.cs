using SmartRecipe.Wpf.ViewModels;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    public partial class CreateRecipeView : UserControl
    {
        public CreateRecipeView(CreateRecipeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        
    }
}