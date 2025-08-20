using SmartRecipe.Domain.Entities;
using SmartRecipe.Infrastructure.Models.Spoonacular;

namespace SmartRecipe.Service.Interface
{
    public interface IEnhancedRecipeService
    {
        /// <summary>
        /// Search recipes from both local database and Spoonacular API
        /// </summary>
        Task<IEnumerable<Recipe>> SearchRecipesAsync(string query, bool includeExternal = true);

        /// <summary>
        /// Import a recipe from Spoonacular and save to local database
        /// </summary>
        Task<Recipe?> ImportRecipeFromSpoonacularAsync(int spoonacularId, Guid userId);

        /// <summary>
        /// Get recipe suggestions based on available ingredients
        /// </summary>
        Task<IEnumerable<SpoonacularRecipeSimple>> GetRecipeSuggestionsAsync(string ingredients);

        /// <summary>
        /// Get random recipes for discovery
        /// </summary>
        Task<IEnumerable<Recipe>> GetRandomRecipesAsync(int count = 5);
    }
}