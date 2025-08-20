using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Domain.Enum;
using SmartRecipe.Service.Interface;
using SmartRecipe.Service.Service.UnitOfWork;

namespace SmartRecipe.Service.Service
{
    public class RecipeService : IRecipeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<RecipeService> _logger;
        private readonly IConfiguration _configuration;

        public RecipeService(
            IUnitOfWork unitOfWork, 
            IFirebaseService firebaseService, 
            ILogger<RecipeService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _firebaseService = firebaseService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Recipe?> GetRecipeByIdAsync(Guid id)
        {
            try
            {
                var recipes = await _unitOfWork.Recipes.GetManyAsync(
                    r => r.Id == id,
                    query => query
                        .Include(r => r.Ingredients)
                        .Include(r => r.Steps.OrderBy(s => s.Order))
                        .Include(r => r.Likes)
                        .Include(r => r.Comments)
                        .Include(r => r.MediaFiles)
                        .Include(r => r.NutritionInfo)
                        .Include(r => r.CreatedByUser)
                );

                return recipes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipe by id {RecipeId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
        {
            try
            {
                return await _unitOfWork.Recipes.GetManyAsync(
                    r => true,
                    query => query
                        .Include(r => r.Likes)
                        .Include(r => r.Comments)
                        .Include(r => r.CreatedByUser)
                        .OrderByDescending(r => r.CreatedAt)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all recipes");
                return new List<Recipe>();
            }
        }

        public async Task<IEnumerable<Recipe>> GetRecentRecipesAsync(int count)
        {
            try
            {
                var recipes = await GetAllRecipesAsync();
                return recipes.Take(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent recipes");
                return new List<Recipe>();
            }
        }

        public async Task<IEnumerable<Recipe>> GetPopularRecipesAsync(int count)
        {
            try
            {
                var recipes = await GetAllRecipesAsync();
                return recipes
                    .OrderByDescending(r => r.LikesCount)
                    .Take(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular recipes");
                return new List<Recipe>();
            }
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            try
            {
                recipe.CreatedAt = DateTime.UtcNow;
                recipe.UpdatedAt = DateTime.UtcNow;
                
                var createdRecipe = await _unitOfWork.Recipes.CreateAsync(recipe);
                await _unitOfWork.SaveChangesAsync();
                
                return recipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe");
                throw;
            }
        }

        public async Task<Recipe> UpdateRecipeAsync(Recipe recipe)
        {
            try
            {
                recipe.UpdatedAt = DateTime.UtcNow;

                var updatedRecipe = await _unitOfWork.Recipes.UpdateAsync(recipe);
                await _unitOfWork.SaveChangesAsync();
                
                return recipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe");
                throw;
            }
        }

        public async Task<bool> DeleteRecipeAsync(Guid id)
        {
            try
            {
                var recipe = await _unitOfWork.Recipes.GetByIdAsync(id);
                if (recipe == null) return false;

                await _unitOfWork.Recipes.RemoveAsync(recipe);
                await _unitOfWork.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe");
                return false;
            }
        }

        public async Task<int> GetTotalRecipesCountAsync()
        {
            try
            {
                var recipes = await GetAllRecipesAsync();
                return recipes.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total recipes count");
                return 0;
            }
        }

        public async Task<int> GetTotalLikesCountAsync()
        {
            try
            {
                var recipes = await GetAllRecipesAsync();
                return recipes.Sum(r => r.LikesCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total likes count");
                return 0;
            }
        }

        // Additional methods for search, categories, etc.
        public async Task<IEnumerable<Recipe>> SearchRecipesAsync(string searchTerm)
        {
            try
            {
                var recipes = await GetAllRecipesAsync();
                return recipes.Where(r => 
                    r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching recipes");
                return new List<Recipe>();
            }
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category)
        {
            try
            {
                if (Enum.TryParse<RecipeCategory>(category, out var categoryEnum))
                {
                    var recipes = await GetAllRecipesAsync();
                    return recipes.Where(r => r.Category == categoryEnum);
                }
                return new List<Recipe>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipes by category");
                return new List<Recipe>();
            }
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByUserAsync(Guid userId)
        {
            try
            {
                var recipes = await GetAllRecipesAsync();
                return recipes.Where(r => r.CreatedByUserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipes by user");
                return new List<Recipe>();
            }
        }

        // Media upload methods
        public async Task<string> UploadRecipeImageAsync(Guid recipeId, byte[] imageData, string fileName)
        {
            try
            {
                var imageUrl = await _firebaseService.UploadImageAsync(imageData, fileName, $"recipes/{recipeId}/images");
                
                var mediaFile = new MediaFile
                {
                    RecipeId = recipeId,
                    FileName = fileName,
                    FileUrl = imageUrl,
                    FileType = MediaType.Image,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.MediaFile.CreateAsync(mediaFile);
                await _unitOfWork.SaveChangesAsync();

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading recipe image");
                throw;
            }
        }

        public async Task<string> UploadRecipeVideoAsync(Guid recipeId, byte[] videoData, string fileName)
        {
            try
            {
                var videoUrl = await _firebaseService.UploadVideoAsync(videoData, fileName, $"recipes/{recipeId}/videos");
                
                var mediaFile = new MediaFile
                {
                    RecipeId = recipeId,
                    FileName = fileName,
                    FileUrl = videoUrl,
                    FileType = MediaType.Video,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.MediaFile.CreateAsync(mediaFile);
                await _unitOfWork.SaveChangesAsync();

                return videoUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading recipe video");
                throw;
            }
        }

        public async Task<bool> DeleteRecipeMediaAsync(Guid recipeId, string mediaUrl)
        {
            try
            {
                var mediaFiles = await _unitOfWork.MediaFile.GetManyAsync(m => m.RecipeId == recipeId && m.FileUrl == mediaUrl);
                var mediaFile = mediaFiles.FirstOrDefault();

                if (mediaFile == null) return false;

                var deleted = await _firebaseService.DeleteFileAsync(mediaUrl);
                if (deleted)
                {
                    await _unitOfWork.MediaFile.RemoveAsync(mediaFile);
                    await _unitOfWork.SaveChangesAsync();
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe media");
                return false;
            }
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByUserIdAsync(Guid userId)
        {
            var recipes = await _unitOfWork.Recipes.GetManyAsync(r => r.CreatedByUserId == userId);
            return recipes;
        }
    }
}