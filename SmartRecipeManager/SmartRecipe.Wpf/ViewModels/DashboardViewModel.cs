using SmartRecipe.Domain.Entities;
using SmartRecipe.Service.Interface;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        
        private ObservableCollection<Recipe> _recentRecipes = new();
        private ObservableCollection<Recipe> _popularRecipes = new();
        private int _totalRecipes;
        private int _totalLikes;
        private bool _isLoading;

        public DashboardViewModel(
            IRecipeService recipeService,
            INavigationService navigationService,
            IAuthenticationService authenticationService)
        {
            _recipeService = recipeService;
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDashboardDataAsync());
            ViewRecipeCommand = new RelayCommand(ViewRecipe);
            CreateRecipeCommand = new RelayCommand(_ => _navigationService.NavigateTo<CreateRecipeView>());
            ViewAllRecipesCommand = new RelayCommand(_ => _navigationService.NavigateTo<RecipeListView>());

            // Load data when dashboard is created
            _ = LoadDashboardDataAsync();
        }

        public ObservableCollection<Recipe> RecentRecipes
        {
            get => _recentRecipes;
            set => SetProperty(ref _recentRecipes, value);
        }

        public ObservableCollection<Recipe> PopularRecipes
        {
            get => _popularRecipes;
            set => SetProperty(ref _popularRecipes, value);
        }

        public int TotalRecipes
        {
            get => _totalRecipes;
            set => SetProperty(ref _totalRecipes, value);
        }

        public int TotalLikes
        {
            get => _totalLikes;
            set => SetProperty(ref _totalLikes, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string WelcomeMessage => $"Welcome back, {_authenticationService.CurrentUser?.UserName}!";

        public ICommand LoadDataCommand { get; }
        public ICommand ViewRecipeCommand { get; }
        public ICommand CreateRecipeCommand { get; }
        public ICommand ViewAllRecipesCommand { get; }

        private async Task LoadDashboardDataAsync()
        {
            IsLoading = true;
            try
            {
                // Load recent recipes
                var recentRecipes = await _recipeService.GetRecentRecipesAsync(6);
                RecentRecipes = new ObservableCollection<Recipe>(recentRecipes);

                // Load popular recipes
                var popularRecipes = await _recipeService.GetPopularRecipesAsync(6);
                PopularRecipes = new ObservableCollection<Recipe>(popularRecipes);

                // Load statistics
                TotalRecipes = await _recipeService.GetTotalRecipesCountAsync();
                TotalLikes = await _recipeService.GetTotalLikesCountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load dashboard data", ex);
            }
            finally
            {
                IsLoading = false;
            }
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