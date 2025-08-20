using SmartRecipe.Infrastructure.Models.Spoonacular;
using SmartRecipe.Service.Interface;
using SmartRecipe.Wpf.Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class SpoonacularRecipeDetailViewModel : BaseViewModel, INavigationAware
    {
        private readonly ISpoonacularService _spoonacularService;
        private readonly IEnhancedRecipeService _enhancedRecipeService;
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;

        private SpoonacularRecipe? _recipe;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isImported;

        public SpoonacularRecipeDetailViewModel(
            ISpoonacularService spoonacularService,
            IEnhancedRecipeService enhancedRecipeService,
            INavigationService navigationService,
            IAuthenticationService authenticationService)
        {
            _spoonacularService = spoonacularService;
            _enhancedRecipeService = enhancedRecipeService;
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            // Commands
            BackCommand = new RelayCommand(_ => _navigationService.NavigateBack());
            ImportRecipeCommand = new AsyncRelayCommand(async _ => await ImportRecipeAsync());
            OpenSourceUrlCommand = new RelayCommand(_ => OpenSourceUrl());
        }

        public SpoonacularRecipe? Recipe
        {
            get => _recipe;
            set => SetProperty(ref _recipe, value);
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

        public bool IsImported
        {
            get => _isImported;
            set => SetProperty(ref _isImported, value);
        }

        public bool IsAuthenticated => _authenticationService?.IsAuthenticated ?? false;

        // Computed properties for easier binding
        public string CleanSummary => Recipe != null ? StripHtmlTags(Recipe.Summary) : string.Empty;
        public string PrimaryDishType => Recipe?.DishTypes?.FirstOrDefault() ?? "Recipe";
        public string CuisineType => Recipe?.Cuisines?.FirstOrDefault() ?? "International";
        public string DietInfo => Recipe?.Diets != null && Recipe.Diets.Any() ? string.Join(", ", Recipe.Diets) : "Not specified";

        // Nutrition helpers
        public double Calories => GetNutrientValue("Calories");
        public double Protein => GetNutrientValue("Protein");
        public double Fat => GetNutrientValue("Fat");
        public double Carbohydrates => GetNutrientValue("Carbohydrates");

        public ICommand BackCommand { get; }
        public ICommand ImportRecipeCommand { get; }
        public ICommand OpenSourceUrlCommand { get; }

        public void OnNavigatedTo(object parameter)
        {
            System.Diagnostics.Debug.WriteLine($"SpoonacularRecipeDetailViewModel: OnNavigatedTo called with parameter: {parameter}");

            // Handle the parameter asynchronously using the UI thread
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    // Set initial loading state
                    IsLoading = true;
                    ErrorMessage = string.Empty;

                    int recipeId;

                    // Check parameter type and try to convert to int
                    if (parameter is int id)
                    {
                        recipeId = id;
                    }
                    else if (parameter != null && int.TryParse(parameter.ToString(), out int parsedId))
                    {
                        recipeId = parsedId;
                    }
                    else
                    {
                        ErrorMessage = "Invalid recipe ID provided.";
                        IsLoading = false;
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"Loading recipe with ID: {recipeId}");

                    // Load the recipe
                    await LoadRecipeAsync(recipeId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnNavigatedTo: {ex.Message}");
                    ErrorMessage = $"Error loading recipe: {ex.Message}";
                    IsLoading = false;
                }
            });
        }

        public async Task LoadRecipeAsync(int recipeId)
        {
            try
            {
                // Loading is already set in OnNavigatedTo
                System.Diagnostics.Debug.WriteLine($"Fetching recipe data for ID: {recipeId}");

                // Load the recipe data
                Recipe = await _spoonacularService.GetRecipeInformationAsync(recipeId);

                if (Recipe == null)
                {
                    ErrorMessage = "Failed to load recipe details.";
                    System.Diagnostics.Debug.WriteLine($"Recipe with ID {recipeId} returned null");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded recipe: {Recipe.Title}");

                    // Force property change notifications for computed properties
                    OnPropertyChanged(nameof(CleanSummary));
                    OnPropertyChanged(nameof(PrimaryDishType));
                    OnPropertyChanged(nameof(CuisineType));
                    OnPropertyChanged(nameof(DietInfo));
                    OnPropertyChanged(nameof(Calories));
                    OnPropertyChanged(nameof(Protein));
                    OnPropertyChanged(nameof(Fat));
                    OnPropertyChanged(nameof(Carbohydrates));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading recipe: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading recipe {recipeId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ImportRecipeAsync()
        {
            if (Recipe == null || !IsAuthenticated)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var importedRecipe = await _enhancedRecipeService.ImportRecipeFromSpoonacularAsync(
                    Recipe.Id,
                    _authenticationService.CurrentUser.Id);

                if (importedRecipe != null)
                {
                    SuccessMessage = "Recipe imported successfully to your collection!";
                    IsImported = true;

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
                    ErrorMessage = "Failed to import recipe. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Import failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenSourceUrl()
        {
            if (!string.IsNullOrEmpty(Recipe?.SourceUrl))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = Recipe.SourceUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Could not open URL: {ex.Message}";
                }
            }
        }

        private double GetNutrientValue(string nutrientName)
        {
            return Recipe?.Nutrition?.Nutrients?
                .FirstOrDefault(n => n.Name.Equals(nutrientName, StringComparison.OrdinalIgnoreCase))?.Amount ?? 0;
        }

        private static string StripHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}