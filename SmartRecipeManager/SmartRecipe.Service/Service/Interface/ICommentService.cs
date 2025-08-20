using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Service.Service.Interface
{
    public interface ICommentService
    {
        Task<Comment> AddCommentAsync(Guid recipeId, Guid userId, string content);
        Task<List<Comment>> GetCommentsByRecipeIdAsync(Guid recipeId);
        Task DeleteCommentAsync(Guid commentId);
    }
}