using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.NutritionInfo
{
    public class NutritionInfoRepository : GenericRepository<SmartRecipe.Domain.Entities.NutritionInfo>, INutritionInfoRepository
    {
        public NutritionInfoRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
