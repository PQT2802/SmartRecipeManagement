using SmartRecipe.Wpf.ViewModels;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Bind PasswordBox to ViewModel
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is LoginViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
        }
    }
}