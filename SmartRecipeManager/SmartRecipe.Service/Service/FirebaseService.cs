using Firebase.Storage;
using System.IO;

namespace SmartRecipe.Service.Service
{
    public interface IFirebaseService
    {
        Task<string> UploadImageAsync(byte[] imageData, string fileName, string folder = "images");
        Task<string> UploadVideoAsync(byte[] videoData, string fileName, string folder = "videos");
        Task<bool> DeleteFileAsync(string fileUrl);
    }

    public class FirebaseService : IFirebaseService
    {
        private readonly string _firebaseBucket;
        private readonly FirebaseStorage _firebaseStorage;

        public FirebaseService(string firebaseBucket)
        {
            _firebaseBucket = firebaseBucket;
            _firebaseStorage = new FirebaseStorage(_firebaseBucket);
        }

        public async Task<string> UploadImageAsync(byte[] imageData, string fileName, string folder = "images")
        {
            try
            {
                var stream = new MemoryStream(imageData);
                var imageUrl = await _firebaseStorage
                    .Child(folder)
                    .Child($"{Guid.NewGuid()}_{fileName}")
                    .PutAsync(stream);

                return imageUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload image: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadVideoAsync(byte[] videoData, string fileName, string folder = "videos")
        {
            try
            {
                var stream = new MemoryStream(videoData);
                var videoUrl = await _firebaseStorage
                    .Child(folder)
                    .Child($"{Guid.NewGuid()}_{fileName}")
                    .PutAsync(stream);

                return videoUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload video: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract file path from URL and delete
                var reference = _firebaseStorage.Child(ExtractPathFromUrl(fileUrl));
                await reference.DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string ExtractPathFromUrl(string url)
        {
            // Extract the file path from Firebase URL
            var uri = new Uri(url);
            return uri.Segments.Last();
        }
    }
}