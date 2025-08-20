using Firebase.Storage;
using SmartRecipe.Domain.Enum;
using System.IO;

namespace SmartRecipe.Wpf.Services
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(string filePath, MediaType mediaType);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, MediaType mediaType);
        Task<bool> DeleteFileAsync(string fileName);
    }

    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly FirebaseStorage _firebaseStorage;

        public FirebaseStorageService(FirebaseStorage firebaseStorage)
        {
            _firebaseStorage = firebaseStorage;
        }

        public async Task<string> UploadFileAsync(string filePath, MediaType mediaType)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            using var fileStream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);
            
            return await UploadFileAsync(fileStream, fileName, mediaType);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, MediaType mediaType)
        {
            try
            {
                var folder = mediaType == MediaType.Image ? "images" : "videos";
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var storagePath = $"{folder}/{uniqueFileName}";

                var uploadTask = _firebaseStorage
                    .Child(storagePath)
                    .PutAsync(fileStream);

                var downloadUrl = await uploadTask;
                return downloadUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Firebase: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                await _firebaseStorage
                    .Child(fileName)
                    .DeleteAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}