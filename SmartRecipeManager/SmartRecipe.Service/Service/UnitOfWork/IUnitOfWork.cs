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
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync();
        Task<int> CompleteAsync();
        // Repositories
        IRecipeRepository Recipes { get; }
        IIngredientRepository Ingredients { get; }
        IStepRepository Steps { get; }
        INutritionInfoRepository NutritionInfos { get; }
        IMediaFileRepository MediaFile { get; }
        IUserRepository Users { get; }
        ICommentRepository Comments { get; }
        ILikeRepository Likes { get; }
    }
}
