using SmartRecipe.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    /// <summary>
    /// Interaction logic for EnhancedRecipeListView.xaml
    /// </summary>
    public partial class EnhancedRecipeListView : UserControl
    {
        public EnhancedRecipeListView(EnhancedRecipeListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure data is loaded when the control becomes visible
            if (DataContext != null && DataContext is ILoadableViewModel loadable)
            {
                loadable.LoadData();
            }
        }
    }

    // Interface to be implemented by the ViewModel
    public interface ILoadableViewModel
    {
        void LoadData();
    }
}