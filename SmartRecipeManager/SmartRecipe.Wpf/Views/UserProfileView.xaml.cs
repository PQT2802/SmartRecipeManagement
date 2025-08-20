using SmartRecipe.Wpf.ViewModels;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    public partial class UserProfileView : UserControl
    {
        // For XAML designer and default instantiation
        public UserProfileView()
        {
            InitializeComponent();
        }

        // For dependency injection
        public UserProfileView(UserProfileViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
