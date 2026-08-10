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

        /// <summary>The dedicated bucket used for job vacancy images.</summary>
        private string VacancyImagesBucketName =>
            _configuration["AWS:VacancyImagesBucketName"]?.Trim() ?? string.Empty;

        private string RegionName =>
            _configuration["AWS:Region"]?.Trim() ?? "us-east-1";

        public async Task<string> UploadAsync(
            IFormFile file,
            string folder,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var key = await UploadInternalAsync(file, folder, userId, BucketName, cancellationToken);
            return key;
        }

        /// <summary>
        /// Uploads a job vacancy image to the dedicated "job-vacancies-images"
        /// bucket (configured via AWS:VacancyImagesBucketName).
        /// </summary>
        public async Task<string> UploadVacancyImageAsync(
            IFormFile file,
            string jobId,
            CancellationToken cancellationToken = default)
        {
            var bucketName = VacancyImagesBucketName;
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new InvalidOperationException(
                    "AWS S3 is not configured. Set AWS:VacancyImagesBucketName in appsettings.json.");
            }

            return await UploadInternalAsync(file, "vacancy-images", jobId, bucketName, cancellationToken);
        }

        private async Task<string> UploadInternalAsync(
            IFormFile file,
            string folder,
            string ownerId,
            string bucketName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new InvalidOperationException(
                    "AWS S3 is not configured. Set AWS:S3BucketName and AWS:Region in appsettings.json.");
            }

            var safeOwnerId = string.Concat(
                ownerId.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_'));

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var key = $"{folder.Trim('/')}/{safeOwnerId}/{Guid.NewGuid():N}{extension}";

            await using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
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

        /// <summary>
        /// Builds a time-limited, signed URL for a private object (e.g. a resume
        /// or certification). Unlike <see cref="GetPublicUrl"/>, this does not
        /// require a public bucket policy — the bucket can stay fully private
        /// and only holders of this specific URL can access the object, and
        /// only until it expires.
        /// </summary>
        public async Task<string> GetPresignedUrlAsync(
            string key,
            TimeSpan expiresIn,
            CancellationToken cancellationToken = default)
        {
            EnsureConfigured();
            return await GetPresignedUrlInternalAsync(key, BucketName, expiresIn, cancellationToken);
        }

        /// <summary>
        /// Builds a time-limited, signed URL for an image stored in the
        /// dedicated vacancy images bucket.
        /// </summary>
        public async Task<string> GetVacancyImagePresignedUrlAsync(
            string key,
            TimeSpan expiresIn,
            CancellationToken cancellationToken = default)
        {
            var bucketName = VacancyImagesBucketName;
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new InvalidOperationException(
                    "AWS S3 is not configured. Set AWS:VacancyImagesBucketName in appsettings.json.");
            }

            return await GetPresignedUrlInternalAsync(key, bucketName, expiresIn, cancellationToken);
        }

        private async Task<string> GetPresignedUrlInternalAsync(
            string key,
            string bucketName,
            TimeSpan expiresIn,
            CancellationToken cancellationToken)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiresIn)
            };

            return await _s3.GetPreSignedURLAsync(request);
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

        /// <summary>Deletes an image from the dedicated vacancy images bucket.</summary>
        public async Task DeleteVacancyImageAsync(
            string? key,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            var bucketName = VacancyImagesBucketName;
            if (string.IsNullOrWhiteSpace(bucketName))
                return;

            await _s3.DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = bucketName,
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
