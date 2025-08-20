using SmartRecipe.Domain.Entities;
using SmartRecipe.Domain.Enum;
using SmartRecipe.Service.Interface;
using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class RecipeListViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private readonly INavigationService _navigationService;

        private ObservableCollection<Recipe> _recipes = new();
        private ObservableCollection<Recipe> _filteredRecipes = new();
        private string _searchText = "";
        private RecipeCategory? _selectedCategory;
        private bool _isLoading;

        public RecipeListViewModel(
            IRecipeService recipeService,
            INavigationService navigationService)
        {
            _recipeService = recipeService;
            _navigationService = navigationService;

            LoadRecipesCommand = new AsyncRelayCommand(async _ => await LoadRecipesAsync());
            SearchCommand = new RelayCommand(_ => FilterRecipes());
            ViewRecipeCommand = new RelayCommand(ViewRecipe);
            CreateRecipeCommand = new RelayCommand(_ => _navigationService.NavigateTo<CreateRecipeView>());

            Categories = Enum.GetValues<RecipeCategory>().Cast<RecipeCategory?>().Prepend(null).ToList();

            // Load recipes when view model is created
            _ = LoadRecipesAsync();
        }

        public ObservableCollection<Recipe> FilteredRecipes
        {
            get => _filteredRecipes;
            set => SetProperty(ref _filteredRecipes, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterRecipes();
                }
            }
        }

        public RecipeCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterRecipes();
                }
            }
        }

        public List<RecipeCategory?> Categories { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadRecipesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ViewRecipeCommand { get; }
        public ICommand CreateRecipeCommand { get; }

        private async Task LoadRecipesAsync()
        {
            IsLoading = true;
            try
            {
                var recipes = await _recipeService.GetAllRecipesAsync();
                _recipes = new ObservableCollection<Recipe>(recipes);
                FilterRecipes();
            }
            catch (Exception ex)
            {
                // Handle error
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterRecipes()
        {
            var filtered = _recipes.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(r =>
                    r.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedCategory.HasValue)
            {
                filtered = filtered.Where(r => r.Category == SelectedCategory.Value);
            }

            FilteredRecipes = new ObservableCollection<Recipe>(filtered);
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