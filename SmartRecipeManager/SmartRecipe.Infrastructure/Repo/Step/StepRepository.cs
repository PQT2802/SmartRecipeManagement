using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.Step
{
    public class StepRepository : GenericRepository<SmartRecipe.Domain.Entities.Step>, IStepRepository
    {
        public StepRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
