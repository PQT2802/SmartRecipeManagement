using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.User
{
    public class UserRepository : GenericRepository<SmartRecipe.Domain.Entities.User>, IUserRepository
    {
        public UserRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
