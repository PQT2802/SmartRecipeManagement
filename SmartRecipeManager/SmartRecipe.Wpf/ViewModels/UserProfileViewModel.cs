using SmartRecipe.Domain.Entities;
using SmartRecipe.Service.Interface;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class UserProfileViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly IRecipeService _recipeService;
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        private User _user;
        private ObservableCollection<Recipe> _userRecipes = new();
        private string _newUsername = "";
        private string _newEmail = "";
        private bool _isEditing;
        private bool _isLoading;
        private string _errorMessage = "";
        private string _successMessage = "";

        public UserProfileViewModel(
            IUserService userService,
            IRecipeService recipeService,
            IAuthenticationService authenticationService,
            INavigationService navigationService)
        {
            _userService = userService;
            _recipeService = recipeService;
            _authenticationService = authenticationService;
            _navigationService = navigationService;

            EditProfileCommand = new RelayCommand(_ => StartEditing());
            SaveProfileCommand = new AsyncRelayCommand(async _ => await SaveProfileAsync());
            CancelEditCommand = new RelayCommand(_ => CancelEditing());
            ViewRecipeCommand = new RelayCommand(ViewRecipe);
            CreateRecipeCommand = new RelayCommand(_ => _navigationService.NavigateTo<CreateRecipeView>());

            LoadUserProfile();
        }

        public User User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public ObservableCollection<Recipe> UserRecipes
        {
            get => _userRecipes;
            set => SetProperty(ref _userRecipes, value);
        }

        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        public string NewEmail
        {
            get => _newEmail;
            set => SetProperty(ref _newEmail, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public ICommand EditProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ViewRecipeCommand { get; }
        public ICommand CreateRecipeCommand { get; }

        private async void LoadUserProfile()
        {
            IsLoading = true;
            try
            {
                User = _authenticationService.CurrentUser;
                if (User != null)
                {
                    NewUsername = User.UserName;
                    NewEmail = User.Email;

                    // Load user's recipes
                    var recipes = await _recipeService.GetRecipesByUserIdAsync(User.Id);
                    UserRecipes = new ObservableCollection<Recipe>(recipes);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load profile: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void StartEditing()
        {
            IsEditing = true;
            ErrorMessage = "";
            SuccessMessage = "";
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewEmail))
            {
                ErrorMessage = "Username and email are required.";
                return;
            }

            IsLoading = true;
            ErrorMessage = "";
            SuccessMessage = "";

            try
            {
                User.UserName = NewUsername;
                User.Email = NewEmail;

                await _userService.UpdateUserAsync(User);
                
                IsEditing = false;
                SuccessMessage = "Profile updated successfully!";
                
                // Clear success message after 3 seconds
                await Task.Delay(3000);
                SuccessMessage = "";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update profile: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEditing()
        {
            IsEditing = false;
            NewUsername = User?.UserName ?? "";
            NewEmail = User?.Email ?? "";
            ErrorMessage = "";
            SuccessMessage = "";
        }

        private void ViewRecipe(object parameter)
        {
            if (parameter is Recipe recipe)
            {
                _navigationService.NavigateTo<RecipeDetailView>(recipe.Id);
            }
        }
    }
}