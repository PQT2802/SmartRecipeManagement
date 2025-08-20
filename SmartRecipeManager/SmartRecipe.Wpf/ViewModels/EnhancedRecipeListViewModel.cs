using SmartRecipe.Infrastructure.Models.Spoonacular;
using SmartRecipe.Service.Interface;
using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class EnhancedRecipeListViewModel : BaseViewModel, INavigationAware
    {
        private readonly ISpoonacularService _spoonacularService;
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRecipeService _recipeService;

        private string _searchText = string.Empty;
        private string? _selectedCategory;
        private string? _selectedCuisine;
        private string? _selectedDiet;
        private ObservableCollection<SpoonacularRecipeSimple> _recipes = new();
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        // Add state tracking
        private bool _isInitialized = false;
        private CancellationTokenSource? _searchCancellationTokenSource;
        private CancellationTokenSource? _loadDataCancellationTokenSource; // New cancellation token source for load data

        public EnhancedRecipeListViewModel(
            ISpoonacularService spoonacularService,
            INavigationService navigationService,
            IAuthenticationService authenticationService,
            IRecipeService recipeService)
        {
            _spoonacularService = spoonacularService;
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _recipeService = recipeService;

            // Initialize category collections
            InitializeCategories();

            // Commands
            LoadDataCommand = new AsyncRelayCommand(async _ => await ForceRefreshAsync());
            SearchCommand = new AsyncRelayCommand(async _ => await SearchRecipesAsync());
            ViewRecipeCommand = new RelayCommand(recipe => ViewRecipe(recipe as SpoonacularRecipeSimple));
            ImportRecipeCommand = new AsyncRelayCommand(async recipe => await ImportRecipeAsync(recipe as SpoonacularRecipeSimple));
            LoadRandomRecipesCommand = new AsyncRelayCommand(async _ => await LoadRandomRecipesAsync());
            CreateRecipeCommand = new RelayCommand(_ => CreateRecipe());
            LoadByCategoryCommand = new AsyncRelayCommand(async _ => await LoadByCategoryAsync());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

            // Add this line to initialize the RefreshCommand
            RefreshCommand = new AsyncRelayCommand(async _ => await ForceRefreshAsync());

            // Load initial data only if not already initialized
            if (!_isInitialized)
            {
                _ = Task.Run(async () => await InitializeDataAsync());
            }
        }

        #region Properties

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Cancel previous search only
                    _searchCancellationTokenSource?.Cancel();
                    _searchCancellationTokenSource = new CancellationTokenSource();

                    // Auto-search with debouncing
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(500, _searchCancellationTokenSource.Token);
                            if (_searchText == value && !_searchCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                await SearchRecipesAsync();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Search was cancelled, ignore
                        }
                    });
                }
            }
        }

        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    _ = Task.Run(async () => await RefreshRecipesAsync());
                }
            }
        }

        public string? SelectedCuisine
        {
            get => _selectedCuisine;
            set
            {
                if (SetProperty(ref _selectedCuisine, value))
                {
                    _ = Task.Run(async () => await RefreshRecipesAsync());
                }
            }
        }

        public string? SelectedDiet
        {
            get => _selectedDiet;
            set
            {
                if (SetProperty(ref _selectedDiet, value))
                {
                    _ = Task.Run(async () => await RefreshRecipesAsync());
                }
            }
        }

        public ObservableCollection<SpoonacularRecipeSimple> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
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

        public bool IsAuthenticated => _authenticationService?.IsAuthenticated ?? false;

        // Category collections for UI binding
        public List<string> Categories { get; private set; } = new();
        public List<string> Cuisines { get; private set; } = new();
        public List<string> Diets { get; private set; } = new();

        #endregion

        #region Commands
        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; } // New refresh command
        public ICommand ViewRecipeCommand { get; }
        public ICommand ImportRecipeCommand { get; }
        public ICommand LoadRandomRecipesCommand { get; }
        public ICommand CreateRecipeCommand { get; }
        public ICommand LoadByCategoryCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        #endregion

        #region Private Methods

        private void InitializeCategories()
        {
            // Spoonacular categories (dish types)
            Categories = new List<string>
            {
                "main course",
                "side dish",
                "dessert",
                "appetizer",
                "salad",
                "bread",
                "breakfast",
                "soup",
                "beverage",
                "sauce",
                "marinade",
                "fingerfood",
                "snack",
                "drink"
            };

            // Popular cuisines
            Cuisines = new List<string>
            {
                "american",
                "italian",
                "chinese",
                "japanese",
                "mexican",
                "indian",
                "french",
                "thai",
                "mediterranean",
                "greek",
                "spanish",
                "korean",
                "vietnamese",
                "middle eastern",
                "german",
                "british"
            };

            // Diet types
            Diets = new List<string>
            {
                "vegetarian",
                "vegan",
                "gluten free",
                "ketogenic",
                "paleo",
                "dairy free",
                "whole30",
                "pescatarian",
                "low carb",
                "low fat",
                "low sodium"
            };
        }

        private async Task InitializeDataAsync()
        {
            _isInitialized = true;
            await LoadRandomRecipesAsync();
        }

        // Fixed: Force refresh method that always reloads data
        private async Task ForceRefreshAsync()
        {
            System.Diagnostics.Debug.WriteLine("Force refresh initiated");

            try
            {
                // Clear any error messages
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = string.Empty;
                    SuccessMessage = string.Empty;
                });

                // Cancel any ongoing searches
                _searchCancellationTokenSource?.Cancel();

                // Set loading state
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = true);

                // Determine what to refresh based on current state
                await RefreshRecipesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Force refresh error: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Failed to refresh: {ex.Message}";
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => IsLoading = false);
            }
        }

        private async Task RefreshRecipesAsync()
        {
            // Determine what type of refresh to do based on current state
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                await SearchRecipesAsync();
            }
            else if (HasActiveFilters())
            {
                await LoadByCategoryAsync();
            }
            else
            {
                await LoadRandomRecipesAsync();
            }
        }

        private bool HasActiveFilters()
        {
            return !string.IsNullOrEmpty(SelectedCategory) ||
                   !string.IsNullOrEmpty(SelectedCuisine) ||
                   !string.IsNullOrEmpty(SelectedDiet);
        }

        private async Task SearchRecipesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadRandomRecipesAsync();
                return;
            }

            // Use the search cancellation token (this one should be cancellable)
            var cancellationToken = _searchCancellationTokenSource?.Token ?? CancellationToken.None;

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;
                });

                System.Diagnostics.Debug.WriteLine($"Searching for: {SearchText}");

                // Pass the search cancellation token
                var results = await _spoonacularService.SearchRecipesAsync(SearchText, 12, 0, cancellationToken);

                if (cancellationToken.IsCancellationRequested) return;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in results.Results)
                    {
                        Recipes.Add(recipe);
                    }
                    System.Diagnostics.Debug.WriteLine($"Found {results.Results.Count} recipes");
                });
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("SearchRecipesAsync was cancelled");
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ErrorMessage = $"Search failed: {ex.Message}";
                    });
                    System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
                }
            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        IsLoading = false;
                    });
                }
            }
        }

        private async Task LoadRandomRecipesAsync()
        {
            // Create a fresh cancellation token for this operation only
            using var operationCancellationSource = new CancellationTokenSource();
            var cancellationToken = operationCancellationSource.Token;

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;
                });

                // Build tags for random recipes based on filters
                var tags = BuildTagsString();
                System.Diagnostics.Debug.WriteLine($"Loading random recipes with tags: {tags ?? "none"}");

                // Pass the fresh cancellation token
                var randomRecipes = await _spoonacularService.GetRandomRecipesAsync(12, tags, cancellationToken);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in randomRecipes)
                    {
                        var simpleRecipe = new SpoonacularRecipeSimple
                        {
                            Id = recipe.Id,
                            Title = recipe.Title,
                            Image = recipe.Image
                        };
                        Recipes.Add(simpleRecipe);
                    }
                    System.Diagnostics.Debug.WriteLine($"Loaded {randomRecipes.Count} random recipes");
                });
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("LoadRandomRecipesAsync was cancelled");
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Failed to load recipes: {ex.Message}";
                });
                System.Diagnostics.Debug.WriteLine($"Load random recipes error: {ex.Message}");
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        private async Task LoadByCategoryAsync()
        {
            // Use a fresh token for category loading (not search token)
            using var operationCancellationSource = new CancellationTokenSource();
            var cancellationToken = operationCancellationSource.Token;

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;
                });

                var tags = BuildTagsString();
                System.Diagnostics.Debug.WriteLine($"Loading recipes by category with tags: {tags ?? "none"}");

                if (string.IsNullOrEmpty(tags))
                {
                    await LoadRandomRecipesAsync();
                    return;
                }

                var recipes = await _spoonacularService.GetRandomRecipesAsync(12, tags, cancellationToken);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in recipes)
                    {
                        var simpleRecipe = new SpoonacularRecipeSimple
                        {
                            Id = recipe.Id,
                            Title = recipe.Title,
                            Image = recipe.Image
                        };
                        Recipes.Add(simpleRecipe);
                    }
                    System.Diagnostics.Debug.WriteLine($"Loaded {recipes.Count} recipes by category");
                });
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("LoadByCategoryAsync was cancelled");
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Failed to load recipes by category: {ex.Message}";
                });
                System.Diagnostics.Debug.WriteLine($"Load by category error: {ex.Message}");
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        private string? BuildTagsString()
        {
            var tags = new List<string>();

            if (!string.IsNullOrEmpty(SelectedCategory))
                tags.Add(SelectedCategory);

            if (!string.IsNullOrEmpty(SelectedCuisine))
                tags.Add(SelectedCuisine);

            if (!string.IsNullOrEmpty(SelectedDiet))
                tags.Add(SelectedDiet);

            return tags.Any() ? string.Join(",", tags) : null;
        }

        private void ClearFilters()
        {
            // Clear search cancellation
            _searchCancellationTokenSource?.Cancel();

            // Clear properties without triggering auto-refresh
            _selectedCategory = null;
            _selectedCuisine = null;
            _selectedDiet = null;
            _searchText = string.Empty;

            // Notify property changes
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(SelectedCuisine));
            OnPropertyChanged(nameof(SelectedDiet));
            OnPropertyChanged(nameof(SearchText));

            // Load random recipes
            _ = Task.Run(LoadRandomRecipesAsync);
        }

        private void ViewRecipe(SpoonacularRecipeSimple? recipe)
        {
            if (recipe != null)
            {
                System.Diagnostics.Debug.WriteLine($"Navigating to recipe details for ID: {recipe.Id}");
                try
                {
                    // Make sure the ID is valid
                    if (recipe.Id <= 0)
                    {
                        ErrorMessage = "Invalid recipe ID.";
                        return;
                    }

                    _navigationService.NavigateTo<SpoonacularRecipeDetailView>(recipe.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    ErrorMessage = $"Failed to open recipe details: {ex.Message}";
                }
            }
        }

        private void CreateRecipe()
        {
            _navigationService.NavigateTo<CreateRecipeView>();
        }

        private async Task ImportRecipeAsync(SpoonacularRecipeSimple? recipe)
        {
            if (recipe == null || !IsAuthenticated)
            {
                if (!IsAuthenticated)
                {
                    ErrorMessage = "Please log in to import recipes.";
                }
                return;
            }

            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;
                    SuccessMessage = string.Empty;
                });

                // Get detailed recipe information with cancellation token
                var detailedRecipe = await _spoonacularService.GetRecipeInformationAsync(
                    recipe.Id, true, CancellationToken.None);

                if (detailedRecipe == null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ErrorMessage = "Failed to get recipe details for import.";
                    });
                    return;
                }

                // Convert to local recipe format
                var localRecipe = await _spoonacularService.ConvertToLocalRecipeAsync(
                    detailedRecipe,
                    _authenticationService.CurrentUser.Id,
                    CancellationToken.None);

                // Save to local database
                var savedRecipe = await _recipeService.CreateRecipeAsync(localRecipe);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (savedRecipe != null)
                    {
                        SuccessMessage = "Recipe imported successfully to your collection!";

                        // Clear success message after 3 seconds
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                SuccessMessage = string.Empty;
                            });
                        });
                    }
                    else
                    {
                        ErrorMessage = "Failed to save imported recipe.";
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Import failed: {ex.Message}";
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        #endregion

        #region Navigation Interface

        // Fixed: Always reload data when navigating to this view
        public void OnNavigatedTo(object parameter)
        {
            System.Diagnostics.Debug.WriteLine("EnhancedRecipeListViewModel: OnNavigatedTo called");

            // Handle search parameter if provided
            if (parameter is string searchQuery && !string.IsNullOrWhiteSpace(searchQuery))
            {
                SearchText = searchQuery;
                return; // SearchText setter will trigger search automatically
            }

            // Always reload data when navigating to this view to ensure fresh content
            // This ensures data is refreshed both on initial navigation and when returning from other pages
            System.Diagnostics.Debug.WriteLine("Reloading data on navigation");

            // Use Dispatcher to ensure UI thread execution
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await ForceRefreshAsync();
            });
        }

        #endregion

        #region Helper Methods

        public void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        public async Task SearchByIngredientsAsync(string ingredients)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;
                });

                // Pass cancellation token
                var results = await _spoonacularService.SearchRecipesByIngredientsAsync(
                    ingredients, 12, CancellationToken.None);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in results.Results)
                    {
                        Recipes.Add(recipe);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Ingredient search failed: {ex.Message}";
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        public async Task LoadSimilarRecipesAsync(int recipeId)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;
                });

                // Pass cancellation token
                var similarRecipes = await _spoonacularService.GetSimilarRecipesAsync(
                    recipeId, 12, CancellationToken.None);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Recipes.Clear();
                    foreach (var recipe in similarRecipes)
                    {
                        Recipes.Add(recipe);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Failed to load similar recipes: {ex.Message}";
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        #endregion
    }
}