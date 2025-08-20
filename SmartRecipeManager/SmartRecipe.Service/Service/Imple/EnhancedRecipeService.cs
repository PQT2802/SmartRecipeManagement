using Microsoft.Extensions.Logging;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Infrastructure.Models.Spoonacular;
using SmartRecipe.Service.Interface;
using SmartRecipe.Service.Service.UnitOfWork;

namespace SmartRecipe.Service.Service.Imple
{
    public class EnhancedRecipeService : IEnhancedRecipeService
    {
        private readonly IRecipeService _recipeService;
        private readonly ISpoonacularService _spoonacularService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EnhancedRecipeService> _logger;

        public EnhancedRecipeService(
            IRecipeService recipeService,
            ISpoonacularService spoonacularService,
            IUnitOfWork unitOfWork,
            ILogger<EnhancedRecipeService> logger)
        {
            _recipeService = recipeService;
            _spoonacularService = spoonacularService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Recipe>> SearchRecipesAsync(string query, bool includeExternal = true)
        {
            var localRecipes = new List<Recipe>();
            var externalRecipes = new List<Recipe>();

            try
            {
                // Search local recipes
                var allLocalRecipes = await _recipeService.GetAllRecipesAsync();
                localRecipes = allLocalRecipes
                    .Where(r => r.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                               r.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Search external recipes if enabled
                if (includeExternal)
                {
                    var spoonacularResults = await _spoonacularService.SearchRecipesAsync(query, 10);

                    foreach (var spoonacularRecipe in spoonacularResults.Results.Take(5)) // Limit external results
                    {
                        try
                        {
                            var detailedRecipe = await _spoonacularService.GetRecipeInformationAsync(spoonacularRecipe.Id);
                            if (detailedRecipe != null)
                            {
                                // Create a temporary user ID for external recipes (you might want to handle this differently)
                                var tempUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                                var convertedRecipe = await _spoonacularService.ConvertToLocalRecipeAsync(detailedRecipe, tempUserId);

                                // Mark as external recipe
                                convertedRecipe.Description = $"[External] {convertedRecipe.Description}";
                                externalRecipes.Add(convertedRecipe);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to convert external recipe {RecipeId}", spoonacularRecipe.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during recipe search for query: {Query}", query);
            }

            // Combine and return results (local first, then external)
            return localRecipes.Concat(externalRecipes);
        }

        public async Task<Recipe?> ImportRecipeFromSpoonacularAsync(int spoonacularId, Guid userId)
        {
            try
            {
                var spoonacularRecipe = await _spoonacularService.GetRecipeInformationAsync(spoonacularId);
                if (spoonacularRecipe == null)
                {
                    _logger.LogWarning("Could not fetch recipe {SpoonacularId} from Spoonacular", spoonacularId);
                    return null;
                }

                var convertedRecipe = await _spoonacularService.ConvertToLocalRecipeAsync(spoonacularRecipe, userId);

                // Save to local database
                var savedRecipe = await _recipeService.CreateRecipeAsync(convertedRecipe);

                _logger.LogInformation("Successfully imported recipe {RecipeId} from Spoonacular", spoonacularId);
                return savedRecipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing recipe {SpoonacularId} from Spoonacular", spoonacularId);
                return null;
            }
        }

        public async Task<IEnumerable<SpoonacularRecipeSimple>> GetRecipeSuggestionsAsync(string ingredients)
        {
            try
            {
                var results = await _spoonacularService.SearchRecipesByIngredientsAsync(ingredients, 6);
                return results.Results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipe suggestions for ingredients: {Ingredients}", ingredients);
                return new List<SpoonacularRecipeSimple>();
            }
        }

        public async Task<IEnumerable<Recipe>> GetRandomRecipesAsync(int count = 5)
        {
            try
            {
                var spoonacularRecipes = await _spoonacularService.GetRandomRecipesAsync(count);
                var convertedRecipes = new List<Recipe>();

                foreach (var spoonacularRecipe in spoonacularRecipes)
                {
                    try
                    {
                        var tempUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                        var convertedRecipe = await _spoonacularService.ConvertToLocalRecipeAsync(spoonacularRecipe, tempUserId);
                        convertedRecipe.Description = $"[Discover] {convertedRecipe.Description}";
                        convertedRecipes.Add(convertedRecipe);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert random recipe {RecipeId}", spoonacularRecipe.Id);
                    }
                }

                return convertedRecipes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random recipes");
                return new List<Recipe>();
            }
        }
    }
}