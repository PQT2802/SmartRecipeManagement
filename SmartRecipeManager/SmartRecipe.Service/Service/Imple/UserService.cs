using Firebase.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Service.Service.UnitOfWork;

namespace SmartRecipe.Service.Service.Imple
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseStorage _firebaseStorage;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, FirebaseStorage firebaseStorage, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _firebaseStorage = firebaseStorage;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _unitOfWork.Users.GetAsync(
                u => u.Id == id,
                include: q => q.Include(u => u.Recipes)
                              .Include(u => u.Comments)
                              .Include(u => u.Likes)
            );
        }

        public async Task UpdateUserProfileAsync(Guid userId, string userName, string email, string profileImagePath)
        {
            try
            {
                var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);
                if (user == null) throw new Exception("User not found");

                user.UserName = userName;
                user.Email = email;

                if (!string.IsNullOrEmpty(profileImagePath) && File.Exists(profileImagePath))
                {
                    var fileName = Path.GetFileName(profileImagePath);
                    var storagePath = $"profiles/{userId}/{fileName}";

                    // Upload to Firebase Storage
                    using var stream = File.OpenRead(profileImagePath);
                    await _firebaseStorage.Child(storagePath).PutAsync(stream);
                    user.ProfileImageUrl = await _firebaseStorage.Child(storagePath).GetDownloadUrlAsync();
                }

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                throw;
            }
        }

        public async Task<List<Recipe>> GetUserRecipesAsync(Guid userId)
        {
            return await _unitOfWork.Recipes.GetManyAsync(
                r => r.CreatedByUserId == userId,
                include: q => q.Include(r => r.Ingredients)
                              .Include(r => r.Steps)
                              .Include(r => r.NutritionInfo)
                              .Include(r => r.MediaFiles)
                              .Include(r => r.Comments)
                              .Include(r => r.Likes)
            );
        }
        public async Task UpdateUserAsync(User user)
        {
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<int> GetUserLikesCountAsync(Guid userId)
        {
            var likes = await _unitOfWork.Likes.GetManyAsync(l => l.UserId == userId);
            return likes.Count;
        }

        public async Task<int> GetUserCommentsCountAsync(Guid userId)
        {
            var comments = await _unitOfWork.Comments.GetManyAsync(c => c.UserId == userId);
            return comments.Count;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetAsync(u => u.Email == email);
            if (user == null) throw new Exception("User not found");
            return user;
        }
        public async Task<int> CreateUserAsync(User user)
        {
            try
            {
                user.Id = Guid.NewGuid();
                await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.CompleteAsync();
                return 1; // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }
    }
}