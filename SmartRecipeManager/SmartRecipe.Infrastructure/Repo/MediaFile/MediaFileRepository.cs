using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Generic;

namespace SmartRecipe.Infrastructure.Repo.MediaFile
{
    public class MediaFileRepository : GenericRepository<SmartRecipe.Domain.Entities.MediaFile>, IMediaFileRepository
    {
        public MediaFileRepository(SmartRecipeDbContext context) : base(context)
        {
        }
    }
}
