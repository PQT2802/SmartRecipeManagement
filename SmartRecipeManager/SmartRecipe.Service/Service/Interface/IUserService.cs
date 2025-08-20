using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Service.Service.Interface
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task UpdateUserProfileAsync(Guid userId, string userName, string email, string profileImagePath);
        Task<List<Recipe>> GetUserRecipesAsync(Guid userId);
        Task<int> GetUserLikesCountAsync(Guid userId);
        Task<int> GetUserCommentsCountAsync(Guid userId);
        Task<int> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}