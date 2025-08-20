using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.Views;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;
        
        private string _email = "";
        private string _password = "";
        private string _errorMessage = "";
        private bool _isLoading;

        public LoginViewModel(
            IAuthenticationService authenticationService,
            INavigationService navigationService)
        {
            _authenticationService = authenticationService;
            _navigationService = navigationService;
            
            LoginCommand = new AsyncRelayCommand(async _ => await LoginAsync());
            NavigateToRegisterCommand = new RelayCommand(_ => NavigateToRegister());
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please fill in all fields.";
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var success = await _authenticationService.LoginAsync(Email, Password);
                if (success)
                {
                    _navigationService.NavigateTo<DashboardView>();
                }
                else
                {
                    ErrorMessage = "Invalid email or password.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateToRegister()
        {
            // Navigate to register view (to be implemented)
        }
    }
}