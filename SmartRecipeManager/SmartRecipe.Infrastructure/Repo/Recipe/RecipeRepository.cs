using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.Recipe
{
    public class RecipeRepository : GenericRepository<SmartRecipe.Domain.Entities.Recipe>, IRecipeRepository
    {
        public RecipeRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
