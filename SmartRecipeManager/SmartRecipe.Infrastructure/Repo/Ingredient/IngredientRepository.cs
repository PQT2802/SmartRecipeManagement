using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.Ingredient
{
    public class IngredientRepository : GenericRepository<SmartRecipe.Domain.Entities.Ingredient>, IIngredientRepository
    {
        public IngredientRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
