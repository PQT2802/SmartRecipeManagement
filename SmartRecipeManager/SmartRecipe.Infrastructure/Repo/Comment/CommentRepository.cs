using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.Comment
{
    public class CommentRepository : GenericRepository<SmartRecipe.Domain.Entities.Comment>, ICommentRepository
    {
        public CommentRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
