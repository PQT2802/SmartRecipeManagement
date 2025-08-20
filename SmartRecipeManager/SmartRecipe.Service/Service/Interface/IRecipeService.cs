using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Service.Interface
{
    public interface IRecipeService
    {
        // Basic CRUD operations
        Task<IEnumerable<Recipe>> GetAllRecipesAsync();
        Task<Recipe?> GetRecipeByIdAsync(Guid id);
        Task<Recipe> CreateRecipeAsync(Recipe recipe);
        Task<Recipe> UpdateRecipeAsync(Recipe recipe);
        Task<bool> DeleteRecipeAsync(Guid id);

        // Dashboard specific methods
        Task<IEnumerable<Recipe>> GetRecentRecipesAsync(int count);
        Task<IEnumerable<Recipe>> GetPopularRecipesAsync(int count);
        Task<int> GetTotalRecipesCountAsync();
        Task<int> GetTotalLikesCountAsync();
        
        // Search and filtering
        Task<IEnumerable<Recipe>> SearchRecipesAsync(string searchTerm);
        Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category);
        Task<IEnumerable<Recipe>> GetRecipesByUserAsync(Guid userId);
        Task<IEnumerable<Recipe>> GetRecipesByUserIdAsync(Guid userId);
        
        // Media handling
        Task<string> UploadRecipeImageAsync(Guid recipeId, byte[] imageData, string fileName);
        Task<string> UploadRecipeVideoAsync(Guid recipeId, byte[] videoData, string fileName);
        Task<bool> DeleteRecipeMediaAsync(Guid recipeId, string mediaUrl);
    }
}