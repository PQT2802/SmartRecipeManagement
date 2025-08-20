using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Service.Service.Interface
{
    public interface ILikeService
    {
        Task<bool> AddLikeAsync(Guid recipeId, Guid userId);
        Task<bool> RemoveLikeAsync(Guid recipeId, Guid userId);
        Task<bool> IsLikedByUserAsync(Guid recipeId, Guid userId);
        Task<int> GetLikesCountAsync(Guid recipeId);
        Task<List<Recipe>> GetTopLikedRecipesAsync(int topN);
        Task<bool> ToggleLikeAsync(Guid recipeId, Guid userId);
    }
}