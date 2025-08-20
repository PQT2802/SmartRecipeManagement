using Firebase.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Domain.Enum;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Service.Service.UnitOfWork;

namespace SmartRecipe.Service.Service.Imple
{
    public class MediaFileService : IMediaFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MediaFileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly FirebaseStorage _firebaseStorage;

        public MediaFileService(
            IUnitOfWork unitOfWork,
            ILogger<MediaFileService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;

            // Initialize Firebase Storage with configuration
            var storageBucket = _configuration["Firebase:StorageBucket"];
            if (string.IsNullOrEmpty(storageBucket))
            {
                throw new InvalidOperationException("Firebase StorageBucket is not configured");
            }

            _firebaseStorage = new FirebaseStorage(storageBucket);
        }

        public async Task<MediaFile> UploadMediaFileAsync(Guid recipeId, string filePath, string fileType)
        {
            try
            {
                _logger.LogInformation("Starting upload for file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileName = Path.GetFileName(filePath);
                var fileExtension = Path.GetExtension(fileName).ToLower();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var storagePath = $"recipes/{recipeId}/{uniqueFileName}";
                var mediaType = fileType.ToLower() == "image" ? MediaType.Image : MediaType.Video;

                _logger.LogInformation("Uploading to Firebase path: {StoragePath}", storagePath);

                // Validate file size (10MB limit)
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    throw new InvalidOperationException("File size cannot exceed 10MB");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov" };
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new InvalidOperationException($"File type {fileExtension} is not supported");
                }

                string downloadUrl;

                // Upload to Firebase Storage
                using (var stream = File.OpenRead(filePath))
                {
                    try
                    {
                        // Upload the file
                        await _firebaseStorage.Child(storagePath).PutAsync(stream);

                        // Get download URL
                        downloadUrl = await _firebaseStorage.Child(storagePath).GetDownloadUrlAsync();

                        _logger.LogInformation("Successfully uploaded file to Firebase: {DownloadUrl}", downloadUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Firebase upload failed for path: {StoragePath}", storagePath);
                        throw new Exception($"Firebase upload failed: {ex.Message}", ex);
                    }
                }

                // Create MediaFile entity
                var mediaFile = new MediaFile
                {
                    Id = Guid.NewGuid(),
                    FileName = fileName,
                    FileUrl = downloadUrl,
                    FileType = mediaType,
                    UploadedAt = DateTime.UtcNow,
                    RecipeId = recipeId
                };

                // Save to database
                // await _unitOfWork.MediaFile.CreateAsync(mediaFile);
                //await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Successfully created MediaFile record: {MediaFileId}", mediaFile.Id);

                return mediaFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading media file for recipe {RecipeId}", recipeId);
                throw;
            }

        }

        public async Task<string> GetMediaFileUrlAsync(Guid mediaFileId)
        {
            try
            {
                var mediaFile = await _unitOfWork.MediaFile.GetAsync(m => m.Id == mediaFileId);
                if (mediaFile == null)
                {
                    throw new FileNotFoundException("Media file not found");
                }

                return mediaFile.FileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media file URL for {MediaFileId}", mediaFileId);
                throw;
            }
        }

        public async Task DeleteMediaFileAsync(Guid mediaFileId)
        {
            try
            {
                var mediaFile = await _unitOfWork.MediaFile.GetAsync(m => m.Id == mediaFileId);
                if (mediaFile == null)
                {
                    throw new FileNotFoundException("Media file not found");
                }

                // Extract storage path from URL
                var storagePath = ExtractStoragePathFromUrl(mediaFile.FileUrl);

                if (!string.IsNullOrEmpty(storagePath))
                {
                    try
                    {
                        await _firebaseStorage.Child(storagePath).DeleteAsync();
                        _logger.LogInformation("Successfully deleted file from Firebase: {StoragePath}", storagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete file from Firebase: {StoragePath}", storagePath);
                        // Continue with database deletion even if Firebase deletion fails
                    }
                }

                // Delete from database
                await _unitOfWork.MediaFile.RemoveAsync(mediaFile);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Successfully deleted MediaFile record: {MediaFileId}", mediaFileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media file {MediaFileId}", mediaFileId);
                throw;
            }
        }

        private string ExtractStoragePathFromUrl(string downloadUrl)
        {
            try
            {
                // Firebase Storage URL format: 
                // https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{path}?{params}
                var uri = new Uri(downloadUrl);
                var pathSegments = uri.Segments;

                // Find the 'o' segment and get everything after it
                var oIndex = Array.IndexOf(pathSegments, "o/");
                if (oIndex >= 0 && oIndex < pathSegments.Length - 1)
                {
                    var encodedPath = pathSegments[oIndex + 1];
                    return Uri.UnescapeDataString(encodedPath.TrimEnd('/'));
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract storage path from URL: {Url}", downloadUrl);
                return string.Empty;
            }
        }
    }
}