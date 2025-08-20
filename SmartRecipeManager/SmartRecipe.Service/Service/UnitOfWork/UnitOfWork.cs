using Microsoft.Extensions.Logging;
using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Comment;
using SmartRecipe.Infrastructure.Repo.Ingredient;
using SmartRecipe.Infrastructure.Repo.Like;
using SmartRecipe.Infrastructure.Repo.MediaFile;
using SmartRecipe.Infrastructure.Repo.NutritionInfo;
using SmartRecipe.Infrastructure.Repo.Recipe;
using SmartRecipe.Infrastructure.Repo.Step;
using SmartRecipe.Infrastructure.Repo.User;

namespace SmartRecipe.Service.Service.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly SmartRecipeDbContext _context;
        public IRecipeRepository Recipes { get; private set; }

        public IIngredientRepository Ingredients { get; private set; }

        public IStepRepository Steps { get; private set; }

        public INutritionInfoRepository NutritionInfos { get; private set; }

        public IMediaFileRepository MediaFile { get; private set; }

        public IUserRepository Users { get; private set; }

        public ICommentRepository Comments { get; private set; }

        public ILikeRepository Likes { get; private set; }


        public UnitOfWork(SmartRecipeDbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = logger;
            _loggerFactory = loggerFactory;
            Recipes = new RecipeRepository(context);
            Ingredients = new IngredientRepository(context);
            Steps = new StepRepository(context);
            NutritionInfos = new NutritionInfoRepository(context);
            MediaFile = new MediaFileRepository(context);
            Users = new UserRepository(context);
            Comments = new CommentRepository(context);
            Likes = new LikeRepository(context);
        }




        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            try
            {
                _context.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing context");
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
