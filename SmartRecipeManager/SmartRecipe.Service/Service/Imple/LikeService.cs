using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Service.Service.UnitOfWork;

namespace SmartRecipe.Service.Service.Imple
{
    public class LikeService : ILikeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LikeService> _logger;

        public LikeService(IUnitOfWork unitOfWork, ILogger<LikeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> AddLikeAsync(Guid recipeId, Guid userId)
        {
            try
            {
                // Check if already liked
                var existingLike = await GetLikeAsync(recipeId, userId);
                if (existingLike != null)
                    return false; // Already liked

                var like = new Like
                {
                    RecipeId = recipeId,
                    UserId = userId
                };

                await _unitOfWork.Likes.CreateAsync(like);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding like for recipe {RecipeId} by user {UserId}", recipeId, userId);
                return false;
            }
        }

        public async Task<bool> RemoveLikeAsync(Guid recipeId, Guid userId)
        {
            try
            {
                var like = await GetLikeAsync(recipeId, userId);
                if (like == null)
                    return false; // Not liked

                await _unitOfWork.Likes.RemoveAsync(like);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing like for recipe {RecipeId} by user {UserId}", recipeId, userId);
                return false;
            }
        }

        public async Task<bool> IsLikedByUserAsync(Guid recipeId, Guid userId)
        {
            try
            {
                var like = await GetLikeAsync(recipeId, userId);
                return like != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if recipe {RecipeId} is liked by user {UserId}", recipeId, userId);
                return false;
            }
        }

        public async Task<int> GetLikesCountAsync(Guid recipeId)
        {
            try
            {
                var likes = await _unitOfWork.Likes.GetAllAsync();
                return likes.Count(l => l.RecipeId == recipeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes count for recipe {RecipeId}", recipeId);
                return 0;
            }
        }

        public async Task<List<Recipe>> GetTopLikedRecipesAsync(int topN)
        {
            try
            {
                var recipes = await _unitOfWork.Recipes.GetAllAsync();
                var likes = await _unitOfWork.Likes.GetAllAsync();

                var topRecipes = recipes
                    .Select(r => new
                    {
                        Recipe = r,
                        LikesCount = likes.Count(l => l.RecipeId == r.Id)
                    })
                    .OrderByDescending(x => x.LikesCount)
                    .Take(topN)
                    .Select(x => x.Recipe)
                    .ToList();

                return topRecipes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top {TopN} liked recipes", topN);
                return new List<Recipe>();
            }
        }

        public async Task<bool> ToggleLikeAsync(Guid recipeId, Guid userId)
        {
            try
            {
                var isLiked = await IsLikedByUserAsync(recipeId, userId);
                
                if (isLiked)
                {
                    return await RemoveLikeAsync(recipeId, userId);
                }
                else
                {
                    return await AddLikeAsync(recipeId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for recipe {RecipeId} by user {UserId}", recipeId, userId);
                return false;
            }
        }

        private async Task<Like?> GetLikeAsync(Guid recipeId, Guid userId)
        {
            var likes = await _unitOfWork.Likes.GetAllAsync();
            return likes.FirstOrDefault(l => l.RecipeId == recipeId && l.UserId == userId);
        }
    }
}