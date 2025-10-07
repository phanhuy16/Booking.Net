using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace BookingApp.Services
{
    public class FirebaseStorageService
    {
        private readonly string _bucketName = "website-42fdb.appspot.com";
        private readonly ILogger<FirebaseStorageService> _logger;
        private readonly StorageClient _storageClient;

        public FirebaseStorageService(ILogger<FirebaseStorageService> logger)
        {
            _logger = logger;

            // Đọc credentials từ file JSON
            var credential = GoogleCredential.FromFile("firebase-service-account.json");
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            var objectName = $"{folder}/{Guid.NewGuid()}_{file.FileName}";
            using var stream = file.OpenReadStream();

            await _storageClient.UploadObjectAsync(_bucketName, objectName, null, stream);
            _logger.LogInformation("Uploaded {FileName} to Firebase Storage as {ObjectName}", file.FileName, objectName);

            // 🔗 Tạo public URL (nếu bucket cho phép public read)
            return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                var objectName = fileUrl.Split($"{_bucketName}/").Last();
                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
                _logger.LogInformation("Deleted file: {ObjectName}", objectName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "File may already be deleted or not found.");
            }
        }
    }
}
