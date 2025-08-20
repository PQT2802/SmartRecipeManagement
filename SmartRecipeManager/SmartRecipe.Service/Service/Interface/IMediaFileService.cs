using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Service.Service.Interface
{
    public interface IMediaFileService
    {
        Task<MediaFile> UploadMediaFileAsync(Guid recipeId, string filePath, string fileType);
        Task<string> GetMediaFileUrlAsync(Guid mediaFileId);
        Task DeleteMediaFileAsync(Guid mediaFileId);
    }
}