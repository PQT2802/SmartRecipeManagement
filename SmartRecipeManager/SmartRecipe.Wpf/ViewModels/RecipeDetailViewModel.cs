using SmartRecipe.Domain.Entities;
using SmartRecipe.Service.Interface;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Wpf.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartRecipe.Wpf.ViewModels
{
    public class RecipeDetailViewModel : BaseViewModel, INavigationAware
    {
        private readonly IRecipeService _recipeService;
        private readonly ILikeService _likeService;
        private readonly ICommentService _commentService;
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        private Recipe _recipe;
        private bool _isLiked;
        private int _likesCount;
        private bool _isLoading; // Added missing property
        private ObservableCollection<Comment> _comments;

        public RecipeDetailViewModel(
            IRecipeService recipeService,
            ILikeService likeService,
            ICommentService commentService,
            IAuthenticationService authenticationService,
            INavigationService navigationService)
        {
            _recipeService = recipeService;
            _likeService = likeService;
            _commentService = commentService;
            _authenticationService = authenticationService;
            _navigationService = navigationService;

            Comments = new ObservableCollection<Comment>();

            // Commands
            ToggleLikeCommand = new RelayCommand(async _ => await ToggleLikeAsync());
            BackCommand = new RelayCommand(_ => _navigationService.NavigateBack());
        }

        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RecipeTitle));
                OnPropertyChanged(nameof(RecipeDescription));
                OnPropertyChanged(nameof(RecipeCategory));
                OnPropertyChanged(nameof(PrepTime));
                OnPropertyChanged(nameof(CookTime));
                OnPropertyChanged(nameof(Servings));
                OnPropertyChanged(nameof(RecipeIngredients));
                OnPropertyChanged(nameof(RecipeSteps));
                OnPropertyChanged(nameof(HasRecipeData));
                OnPropertyChanged(nameof(AuthorName));
                OnPropertyChanged(nameof(CreatedDate));

                if (_recipe != null)
                {
                    LikesCount = _recipe.LikesCount; // Use the computed property
                }
            }
        }

        // Display properties that handle null values
        public string RecipeTitle => Recipe?.Title ?? "Recipe not available";
        public string RecipeDescription => Recipe?.Description ?? "No description available";
        public string RecipeCategory => Recipe?.Category.ToString() ?? "No category";
        public string PrepTime => Recipe?.PrepTimeMinutes.ToString() ?? "0";
        public string CookTime => Recipe?.CookTimeMinutes.ToString() ?? "0";
        public string Servings => Recipe?.Servings.ToString() ?? "0";
        public string AuthorName => Recipe?.CreatedByUser?.UserName ?? "Unknown";
        public string CreatedDate => Recipe?.CreatedAt.ToString("MMM dd, yyyy") ?? "Unknown date";

        public ICollection<Ingredient> RecipeIngredients => Recipe?.Ingredients ?? new List<Ingredient>();
        public ICollection<Step> RecipeSteps => Recipe?.Steps ?? new List<Step>();
        public bool HasRecipeData => Recipe != null;

        public bool IsLiked
        {
            get => _isLiked;
            set
            {
                _isLiked = value;
                OnPropertyChanged();
            }
        }

        public int LikesCount
        {
            get => _likesCount;
            set
            {
                _likesCount = value;
                OnPropertyChanged();
            }
        }

        // Added missing IsLoading property
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Comment> Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleLikeCommand { get; }
        public ICommand BackCommand { get; }

        // Implement INavigationAware interface
        public async void OnNavigatedTo(object parameter)
        {
            System.Diagnostics.Debug.WriteLine($"RecipeDetailViewModel: OnNavigatedTo called with parameter: {parameter}");

            if (parameter is Guid recipeId)
            {
                await LoadRecipeAsync(recipeId);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"RecipeDetailViewModel: Invalid parameter type. Expected Guid, got {parameter?.GetType()}");
                // Set to null to show "not available" messages
                Recipe = null;
            }
        }

        public async Task LoadRecipeAsync(Guid recipeId)
        {
            try
            {
                IsLoading = true; // Set loading to true when starting
                System.Diagnostics.Debug.WriteLine($"Loading recipe with ID: {recipeId}");

                Recipe = await _recipeService.GetRecipeByIdAsync(recipeId);

                if (Recipe != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Recipe loaded: {Recipe.Title}");

                    if (_authenticationService.IsAuthenticated)
                    {
                        // Load like status
                        IsLiked = await _likeService.IsLikedByUserAsync(Recipe.Id, _authenticationService.CurrentUser.Id);

                        // Load actual likes count from service
                        LikesCount = await _likeService.GetLikesCountAsync(Recipe.Id);

                        // Load comments
                        var comments = await _commentService.GetCommentsByRecipeIdAsync(Recipe.Id);
                        Comments.Clear();
                        foreach (var comment in comments)
                        {
                            Comments.Add(comment);
                        }
                    }
                    else
                    {
                        // Even if not authenticated, show recipe data but without like status
                        LikesCount = await _likeService.GetLikesCountAsync(Recipe.Id);

                        // Load comments (public data)
                        var comments = await _commentService.GetCommentsByRecipeIdAsync(Recipe.Id);
                        Comments.Clear();
                        foreach (var comment in comments)
                        {
                            Comments.Add(comment);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Recipe not found");
                    // Recipe not found, clear any existing data
                    LikesCount = 0;
                    Comments.Clear();
                }
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error loading recipe: {ex.Message}");
                // Set Recipe to null to trigger "not have" display
                Recipe = null;
                LikesCount = 0;
                Comments.Clear();
            }
            finally
            {
                IsLoading = false; // Set loading to false when complete
            }
        }

        private async Task ToggleLikeAsync()
        {
            if (!_authenticationService.IsAuthenticated || Recipe == null)
                return;

            try
            {
                bool success;
                if (IsLiked)
                {
                    success = await _likeService.RemoveLikeAsync(Recipe.Id, _authenticationService.CurrentUser.Id);
                    if (success)
                    {
                        LikesCount--;
                        IsLiked = false;
                    }
                }
                else
                {
                    success = await _likeService.AddLikeAsync(Recipe.Id, _authenticationService.CurrentUser.Id);
                    if (success)
                    {
                        LikesCount++;
                        IsLiked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error toggling like: {ex.Message}");
            }
        }
    }
}