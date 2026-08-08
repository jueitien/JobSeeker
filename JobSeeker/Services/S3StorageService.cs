using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;

namespace JobSeeker.Services
{
    public class S3StorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly IConfiguration _configuration;

        public S3StorageService(IAmazonS3 s3, IConfiguration configuration)
        {
            _s3 = s3;
            _configuration = configuration;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(BucketName);

        private string BucketName =>
            _configuration["AWS:S3BucketName"]?.Trim() ?? string.Empty;

        private string RegionName =>
            _configuration["AWS:Region"]?.Trim() ?? "us-east-1";

        public async Task<string> UploadAsync(
            IFormFile file,
            string folder,
            string userId,
            CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            var safeUserId = string.Concat(
                userId.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_'));

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var key = $"{folder.Trim('/')}/{safeUserId}/{Guid.NewGuid():N}{extension}";

            await using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = key,
                InputStream = stream,
                ContentType = GetContentType(extension)
            };

            // Public access is controlled by the S3 bucket policy.
            // No object ACL is used because modern S3 buckets commonly have ACLs disabled.
            await _s3.PutObjectAsync(request, cancellationToken);
            return key;
        }

        public string GetPublicUrl(string key)
        {
            EnsureConfigured();

            var encodedKey = string.Join("/",
                key.Split('/', StringSplitOptions.RemoveEmptyEntries)
                   .Select(Uri.EscapeDataString));

            return $"https://{BucketName}.s3.{RegionName}.amazonaws.com/{encodedKey}";
        }

        public async Task DeleteAsync(
            string? key,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            EnsureConfigured();

            await _s3.DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = BucketName,
                    Key = key
                },
                cancellationToken);
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(BucketName))
            {
                throw new InvalidOperationException(
                    "AWS S3 is not configured. Set AWS:S3BucketName and AWS:Region in appsettings.json.");
            }
        }

        public static string GetContentType(string? extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
