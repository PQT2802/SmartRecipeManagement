using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.Recipe
{
    public interface IRecipeRepository : IGenericRepository<SmartRecipe.Domain.Entities.Recipe>
    {
    }
}
