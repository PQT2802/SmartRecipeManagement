using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.Like
{
    public class LikeRepository : GenericRepository<SmartRecipe.Domain.Entities.Like>, ILikeRepository
    {
        public LikeRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
