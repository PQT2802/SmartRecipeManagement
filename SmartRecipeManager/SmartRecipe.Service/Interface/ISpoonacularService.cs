using SmartRecipe.Infrastructure.Models.Spoonacular;

namespace SmartRecipe.Service.Interface
{
    public interface ISpoonacularService
    {
        /// <summary>
        /// Search for recipes by query
        /// </summary>
        Task<SpoonacularSearchResponse> SearchRecipesAsync(
       string query,
       int number = 10,
       int offset = 0,
       CancellationToken cancellationToken = default);

        /// <summary>
        /// Get detailed recipe information by ID
        /// </summary>
        Task<SpoonacularRecipe?> GetRecipeInformationAsync(
    int id,
    bool includeNutrition = true,
    CancellationToken cancellationToken = default);

        /// <summary>
        /// Get random recipes
        /// </summary>
        Task<List<SpoonacularRecipe>> GetRandomRecipesAsync(
    int number = 1,
    string? tags = null,
    CancellationToken cancellationToken = default);

        /// <summary>
        /// Search recipes by ingredients
        /// </summary>
        Task<SpoonacularSearchResponse> SearchRecipesByIngredientsAsync(
    string ingredients,
    int number = 10,
    CancellationToken cancellationToken = default);

        /// <summary>
        /// Get similar recipes
        /// </summary>
        Task<List<SpoonacularRecipeSimple>> GetSimilarRecipesAsync(
    int id,
    int number = 3,
    CancellationToken cancellationToken = default);

        /// <summary>
        /// Get recipe nutrition information
        /// </summary>
        Task<SpoonacularNutrition?> GetRecipeNutritionAsync(
    int id,
    CancellationToken cancellationToken = default);

        /// <summary>
        /// Convert Spoonacular recipe to domain recipe
        /// </summary>
        Task<Domain.Entities.Recipe> ConvertToLocalRecipeAsync(
    SpoonacularRecipe spoonacularRecipe,
    Guid userId,
    CancellationToken cancellationToken = default);
    }
}