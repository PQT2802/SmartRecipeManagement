using Microsoft.Win32;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Domain.Enum;
using SmartRecipe.Service.Interface;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class CreateRecipeViewModel : BaseViewModel
    {
        private readonly IRecipeService _recipeService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        private string _title = "";
        private string _description = "";
        private RecipeCategory _selectedCategory;
        private int _cookingTime;
        private int _prepTime;
        private int _cookTime;
        private ObservableCollection<IngredientInput> _ingredients = new();
        private ObservableCollection<StepInput> _steps = new();
        private string _selectedImagePath = "";
        private bool _isLoading;
        private string _errorMessage = "";

        public CreateRecipeViewModel(
            IRecipeService recipeService,
            IMediaFileService mediaFileService,
            IAuthenticationService authenticationService,
            INavigationService navigationService)
        {
            _recipeService = recipeService;
            _mediaFileService = mediaFileService;
            _authenticationService = authenticationService;
            _navigationService = navigationService;

            Categories = Enum.GetValues<RecipeCategory>().ToList();
            SelectedCategory = Categories.First();

            // Initialize with empty ingredient and step
            Ingredients.Add(new IngredientInput());
            Steps.Add(new StepInput { StepNumber = 1 });

            AddIngredientCommand = new RelayCommand(_ => AddIngredient());
            RemoveIngredientCommand = new RelayCommand(RemoveIngredient);
            AddStepCommand = new RelayCommand(_ => AddStep());
            RemoveStepCommand = new RelayCommand(RemoveStep);
            SelectImageCommand = new RelayCommand(_ => SelectImage());
            SaveRecipeCommand = new AsyncRelayCommand(async _ => await SaveRecipeAsync());
            CancelCommand = new RelayCommand(_ => _navigationService.NavigateBack());
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public RecipeCategory SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public int CookingTime
        {
            get => _cookingTime;
            set => SetProperty(ref _cookingTime, value);
        }

        public int PrepTime
        {
            get => _prepTime;
            set => SetProperty(ref _prepTime, value);
        }

        public int CookTime
        {
            get => _cookTime;
            set => SetProperty(ref _cookTime, value);
        }

        public ObservableCollection<IngredientInput> Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        public ObservableCollection<StepInput> Steps
        {
            get => _steps;
            set => SetProperty(ref _steps, value);
        }

        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set => SetProperty(ref _selectedImagePath, value);
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

        public List<RecipeCategory> Categories { get; }

        public ICommand AddIngredientCommand { get; }
        public ICommand RemoveIngredientCommand { get; }
        public ICommand AddStepCommand { get; }
        public ICommand RemoveStepCommand { get; }
        public ICommand SelectImageCommand { get; }
        public ICommand SaveRecipeCommand { get; }
        public ICommand CancelCommand { get; }

        private void AddIngredient()
        {
            Ingredients.Add(new IngredientInput());
        }

        private void RemoveIngredient(object parameter)
        {
            if (parameter is IngredientInput ingredient && Ingredients.Count > 1)
            {
                Ingredients.Remove(ingredient);
            }
        }

        private void AddStep()
        {
            Steps.Add(new StepInput { StepNumber = Steps.Count + 1 });
        }

        private void RemoveStep(object parameter)
        {
            if (parameter is StepInput step && Steps.Count > 1)
            {
                Steps.Remove(step);
                // Renumber steps
                for (int i = 0; i < Steps.Count; i++)
                {
                    Steps[i].StepNumber = i + 1;
                }
            }
        }

        private void SelectImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Select Recipe Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedImagePath = openFileDialog.FileName;
            }
        }

        private async Task SaveRecipeAsync()
        {
            if (!ValidateInput())
                return;

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var ingredients = Ingredients.Where(i => !string.IsNullOrWhiteSpace(i.Name))
                    .Select(i => new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        Name = i.Name,
                        Quantity = i.Quantity
                    }).ToList();

                var steps = Steps.Where(s => !string.IsNullOrWhiteSpace(s.Description))
                    .Select((s, index) => new Step
                    {
                        Id = Guid.NewGuid(),
                        Order = index + 1, // Fixed: Use Order property instead of StepNumber
                        Description = s.Description
                    }).ToList();

                var recipe = new Recipe
                {
                    Id = Guid.NewGuid(),
                    Title = Title,
                    Description = Description,
                    Category = SelectedCategory,
                    PrepTimeMinutes = PrepTime, // Fixed: Use PrepTime property
                    CookTimeMinutes = CookTime, // Fixed: Use CookTime property
                    CreatedByUserId = _authenticationService.CurrentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Ingredients = ingredients,
                    Steps = steps
                };

                // Upload image if selected
                if (!string.IsNullOrWhiteSpace(SelectedImagePath))
                {
                    try
                    {
                        var mediaFile = await _mediaFileService.UploadMediaFileAsync(recipe.Id, SelectedImagePath, "image");
                        recipe.MediaFiles = new List<MediaFile> { mediaFile };
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Recipe created successfully, but image upload failed: {ex.Message}";
                        // Don't fail recipe creation if image upload fails
                    }
                }

                await _recipeService.CreateRecipeAsync(recipe);
                _navigationService.NavigateTo<RecipeDetailView>(recipe.Id);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save recipe: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            // Add authentication check first
            if (_authenticationService?.CurrentUser == null)
            {
                ErrorMessage = "You must be logged in to create a recipe.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Please enter a recipe title.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ErrorMessage = "Please enter a recipe description.";
                return false;
            }

            // Fixed: Check PrepTime and CookTime instead of CookingTime
            if (PrepTime <= 0 && CookTime <= 0)
            {
                ErrorMessage = "Please enter valid prep time or cooking time.";
                return false;
            }

            if (!Ingredients.Any(i => !string.IsNullOrWhiteSpace(i.Name)))
            {
                ErrorMessage = "Please add at least one ingredient.";
                return false;
            }

            if (!Steps.Any(s => !string.IsNullOrWhiteSpace(s.Description)))
            {
                ErrorMessage = "Please add at least one step.";
                return false;
            }

            return true;
        }
    }

    public class IngredientInput : BaseViewModel
    {
        private string _name = "";
        private string _quantity = "";

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
    }

    public class StepInput : BaseViewModel
    {
        private int _stepNumber;
        private string _description = "";

        public int StepNumber
        {
            get => _stepNumber;
            set => SetProperty(ref _stepNumber, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
    }
}