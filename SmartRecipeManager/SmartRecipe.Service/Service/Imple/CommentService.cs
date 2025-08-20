using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Service.Service.UnitOfWork;

namespace SmartRecipe.Service.Service.Imple
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IUnitOfWork unitOfWork, ILogger<CommentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Comment> AddCommentAsync(Guid recipeId, Guid userId, string content)
        {
            try
            {
                var comment = new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    RecipeId = recipeId,
                    UserId = userId
                };

                await _unitOfWork.Comments.CreateAsync(comment);
                await _unitOfWork.CompleteAsync();

                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment");
                throw;
            }
        }

        public async Task<List<Comment>> GetCommentsByRecipeIdAsync(Guid recipeId)
        {
            return await _unitOfWork.Comments.GetManyAsync(
                c => c.RecipeId == recipeId,
                include: q => q.Include(c => c.User)
            );
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            try
            {
                var comment = await _unitOfWork.Comments.GetAsync(c => c.Id == commentId);
                if (comment == null) throw new Exception("Comment not found");

                await _unitOfWork.Comments.RemoveAsync(comment);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                throw;
            }
        }
    }
}