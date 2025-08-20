using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.Views;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private object _currentView;

        public MainViewModel(
            INavigationService navigationService,
            IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            // Initialize commands
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateToDashboard());
            NavigateToRecipesCommand = new RelayCommand(_ => NavigateToRecipes());
            NavigateToCreateRecipeCommand = new RelayCommand(_ => NavigateToCreateRecipe());
            NavigateToLoginCommand = new RelayCommand(_ => NavigateToLogin());
            NavigateToProfileCommand = new RelayCommand(_ => NavigateToProfile());
            NavigateToEnhancedRecipesCommand = new RelayCommand(_ => NavigateToEnhancedRecipes()); // Fixed: Moved inside constructor
            LogoutCommand = new RelayCommand(_ => Logout());
            GoBackCommand = new RelayCommand(_ => _navigationService.NavigateBack());

            // Subscribe to navigation changes but DON'T navigate yet
            if (_navigationService is NavigationService navService)
            {
                navService.CurrentViewChanged += OnCurrentViewChanged;
            }

            // Subscribe to authentication changes
            if (_authenticationService != null)
            {
                _authenticationService.UserChanged += OnUserChanged;
            }
        }

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public bool IsAuthenticated => _authenticationService?.IsAuthenticated ?? false;

        public Domain.Entities.User CurrentUser => _authenticationService?.CurrentUser;

        // Commands
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToRecipesCommand { get; }
        public ICommand NavigateToCreateRecipeCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToProfileCommand { get; }
        public ICommand NavigateToEnhancedRecipesCommand { get; } // Fixed: Proper declaration
        public ICommand LogoutCommand { get; }
        public ICommand GoBackCommand { get; }

        // Call this after UI is loaded
        public void Initialize()
        {
            if (IsAuthenticated)
            {
                NavigateToDashboard();
            }
            else
            {
                NavigateToLogin();
            }
        }

        private void NavigateToDashboard()
        {
            if (IsAuthenticated)
            {
                _navigationService.NavigateTo<DashboardView>();
            }
            else
            {
                NavigateToLogin();
            }
        }

        private void NavigateToRecipes()
        {
            if (IsAuthenticated)
            {
                _navigationService.NavigateTo<RecipeListView>();
            }
            else
            {
                NavigateToLogin();
            }
        }

        private void NavigateToCreateRecipe()
        {
            if (IsAuthenticated)
            {
                _navigationService.NavigateTo<CreateRecipeView>();
            }
            else
            {
                NavigateToLogin();
            }
        }

        private void NavigateToLogin()
        {
            _navigationService.NavigateTo<LoginView>();
        }

        private void NavigateToProfile()
        {
            if (IsAuthenticated)
            {
                _navigationService.NavigateTo<UserProfileView>();
            }
            else
            {
                NavigateToLogin();
            }
        }

        // Fixed: Added the missing method
        private void NavigateToEnhancedRecipes()
        {
            if (IsAuthenticated)
            {
                _navigationService.NavigateTo<EnhancedRecipeListView>();
            }
            else
            {
                NavigateToLogin();
            }
        }

        private async void Logout()
        {
            await _authenticationService?.LogoutAsync();
            NavigateToLogin();
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(CurrentUser));
        }

        private void OnCurrentViewChanged(object newView)
        {
            CurrentView = newView;
        }

        private void OnUserChanged(Domain.Entities.User user)
        {
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(CurrentUser));
        }
    }
}