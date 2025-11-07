using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;

namespace TheVehicleEcosystemAPI.Utils
{
    public class CloudflareR2Storage
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _accountId;
        private readonly ILogger<CloudflareR2Storage> _logger;
        private readonly string _publicUrl;

        public CloudflareR2Storage(IConfiguration configuration, ILogger<CloudflareR2Storage> logger)
        {
            _logger = logger;
            _bucketName = configuration["R2Settings:BucketName"]
                ?? throw new ArgumentNullException("BucketName", "R2Settings:BucketName is not configured");
            _accountId = configuration["R2Settings:AccountId"]
                ?? throw new ArgumentNullException("AccountId", "R2Settings:AccountId is not configured");

            var accessKey = configuration["R2Settings:AccessKey"]
                ?? throw new ArgumentNullException("AccessKey", "R2Settings:AccessKey is not configured");
            var secretKey = configuration["R2Settings:SecretKey"]
                ?? throw new ArgumentNullException("SecretKey", "R2Settings:SecretKey is not configured");

            // Get Public URL from configuration
            _publicUrl = configuration["R2Settings:PublicURL"];
            if (string.IsNullOrEmpty(_publicUrl))
            {
                // Fallback to auto-generated URL if not configured
                _publicUrl = $"https://pub-{_accountId}.r2.dev/{_bucketName}";
                _logger.LogWarning("R2Settings:PublicURL not configured. Using auto-generated URL: {PublicUrl}", _publicUrl);
            }
            else
            {
                // Remove trailing slash if present
                _publicUrl = _publicUrl.TrimEnd('/');
                _logger.LogInformation("Using configured R2 Public URL: {PublicUrl}", _publicUrl);
            }

            // Cloudflare R2 endpoint
            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{_accountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _s3Client = new AmazonS3Client(credentials, config);
        }

        /// <summary>
        /// Upload an image to Cloudflare R2 from IFormFile
        /// </summary>
        /// <param name="formFile">The uploaded form file</param>
        /// <param name="folder">Optional folder path within bucket (e.g., "vehicles")</param>
        /// <returns>The public URL of the uploaded image</returns>
        public async Task<string> UploadImageAsync(IFormFile formFile, string? folder = null)
        {
            if (formFile == null || formFile.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(formFile));
            }

            try
            {
                // Generate unique file name
                var fileExtension = Path.GetExtension(formFile.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var key = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";

                using var stream = formFile.OpenReadStream();
                return await UploadImageAsync(stream, key, formFile.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image from IFormFile");
                throw new InvalidOperationException("Failed to upload image to R2", ex);
            }
        }

        /// <summary>
        /// Upload an image to Cloudflare R2 from base64 string
        /// </summary>
        /// <param name="base64Image">Base64 encoded image string (with or without data URI prefix)</param>
        /// <param name="folder">Optional folder path within bucket</param>
        /// <param name="fileName">Optional file name</param>
        /// <returns>The public URL of the uploaded image</returns>
        public async Task<string> UploadImageAsync(string base64Image, string? folder = null, string? fileName = null)
        {
            try
            {
                // Remove data URI prefix if present
                string contentType = "image/jpeg"; // default
                if (base64Image.Contains(","))
                {
                    var parts = base64Image.Split(',');
                    var header = parts[0];
                    base64Image = parts[1];

                    // Extract content type from data URI
                    if (header.Contains("image/"))
                    {
                        var typeStart = header.IndexOf("image/");
                        var typeEnd = header.IndexOf(";", typeStart);
                        if (typeEnd > typeStart)
                        {
                            contentType = header.Substring(typeStart, typeEnd - typeStart);
                        }
                    }
                }

                // Convert base64 to byte array
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // Generate file name if not provided
                if (string.IsNullOrEmpty(fileName))
                {
                    var extension = GetExtensionFromContentType(contentType);
                    fileName = $"{Guid.NewGuid()}{extension}";
                }

                var key = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";

                using var stream = new MemoryStream(imageBytes);
                return await UploadImageAsync(stream, key, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image from base64 string");
                throw new InvalidOperationException("Failed to upload image to R2", ex);
            }
        }

        /// <summary>
        /// Upload an image to Cloudflare R2 from byte array
        /// </summary>
        /// <param name="imageBytes">Image data as byte array</param>
        /// <param name="fileName">File name for the image</param>
        /// <param name="folder">Optional folder path within bucket</param>
        /// <param name="contentType">Content type of the image</param>
        /// <returns>The public URL of the uploaded image</returns>
        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName, string? folder = null, string contentType = "image/jpeg")
        {
            try
            {
                var key = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";

                using var stream = new MemoryStream(imageBytes);
                return await UploadImageAsync(stream, key, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image from byte array");
                throw new InvalidOperationException("Failed to upload image to R2", ex);
            }
        }

        /// <summary>
        /// Upload an image to Cloudflare R2 from stream
        /// </summary>
        /// <param name="stream">Stream containing image data</param>
        /// <param name="key">Object key (path) in the bucket</param>
        /// <param name="contentType">Content type of the image</param>
        /// <returns>The public URL of the uploaded image</returns>
        private async Task<string> UploadImageAsync(Stream stream, string key, string contentType)
        {
            try
            {
                // Read stream into memory to avoid chunked encoding issues with R2
                byte[] fileBytes;
                if (stream is MemoryStream memStream)
                {
                    fileBytes = memStream.ToArray();
                }
                else
                {
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }

                using var memoryStream = new MemoryStream(fileBytes);

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = memoryStream,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.PublicRead, // Make object publicly readable
                    DisablePayloadSigning = true, // Important for R2 compatibility
                    UseChunkEncoding = false // Disable chunked encoding for R2
                };

                var response = await _s3Client.PutObjectAsync(putRequest);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InvalidOperationException($"Failed to upload to R2. Status: {response.HttpStatusCode}");
                }

                // Construct public URL
                var publicUrl = $"{_publicUrl}/{key}";
                _logger.LogInformation("Image uploaded successfully to R2: {Key}", key);

                return publicUrl;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 error uploading to R2: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to upload to R2: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading to R2");
                throw new InvalidOperationException("Failed to upload image to R2", ex);
            }
        }

        /// <summary>
        /// Download an image from R2
        /// </summary>
        /// <param name="imageUrl">The URL or key of the image</param>
        /// <returns>Image data as byte array</returns>
        public async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            try
            {
                // Extract key from URL if full URL is provided
                var key = ExtractKeyFromUrl(imageUrl);

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);

                _logger.LogInformation("Image downloaded successfully from R2: {Key}", key);
                return memoryStream.ToArray();
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error downloading image from R2: {ImageUrl}", imageUrl);
                throw new InvalidOperationException($"Failed to download image from R2: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Download an image and return as base64 string
        /// </summary>
        /// <param name="imageUrl">The URL or key of the image</param>
        /// <returns>Base64 encoded image string</returns>
        public async Task<string> DownloadImageAsBase64Async(string imageUrl)
        {
            var imageBytes = await DownloadImageAsync(imageUrl);
            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Delete an image from R2
        /// </summary>
        /// <param name="imageUrl">The URL or key of the image to delete</param>
        /// <returns>True if deletion was successful</returns>
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                // Extract key from URL if full URL is provided
                var key = ExtractKeyFromUrl(imageUrl);

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent ||
                    response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Image deleted successfully from R2: {Key}", key);
                    return true;
                }

                _logger.LogWarning("Unexpected status code when deleting from R2: {StatusCode}", response.HttpStatusCode);
                return false;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from R2: {ImageUrl}", imageUrl);
                return false;
            }
        }

        /// <summary>
        /// Check if an image exists in R2
        /// </summary>
        /// <param name="imageUrl">The URL or key of the image</param>
        /// <returns>True if the image exists</returns>
        public async Task<bool> ImageExistsAsync(string imageUrl)
        {
            try
            {
                var key = ExtractKeyFromUrl(imageUrl);

                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        /// <summary>
        /// Extract object key from full R2 URL
        /// </summary>
        /// <param name="urlOrKey">Full URL or just the key</param>
        /// <returns>Object key</returns>
        private string ExtractKeyFromUrl(string urlOrKey)
        {
            // If it's already just a key (no protocol), return as-is
            if (!urlOrKey.StartsWith("http://") && !urlOrKey.StartsWith("https://"))
            {
                return urlOrKey;
            }

            // Extract key from URL
            try
            {
                var uri = new Uri(urlOrKey);
                var path = uri.AbsolutePath.TrimStart('/');

                // If path starts with bucket name, remove it (for backward compatibility with old URLs)
                if (path.StartsWith($"{_bucketName}/"))
                {
                    path = path.Substring(_bucketName.Length + 1);
                }

                return path;
            }
            catch
            {
                // If parsing fails, assume it's already a key
                return urlOrKey;
            }
        }

        /// <summary>
        /// Get file extension from content type
        /// </summary>
        private string GetExtensionFromContentType(string contentType)
        {
            return contentType.ToLower() switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                _ => ".jpg"
            };
        }

        /// <summary>
        /// List all images in a folder
        /// </summary>
        /// <param name="folder">Folder path</param>
        /// <returns>List of image URLs</returns>
        public async Task<List<string>> ListImagesAsync(string? folder = null)
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = folder
                };

                var response = await _s3Client.ListObjectsV2Async(request);
                var urls = response.S3Objects
                    .Select(obj => $"{_publicUrl}/{obj.Key}")
                    .ToList();

                return urls;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error listing images from R2");
                throw new InvalidOperationException("Failed to list images from R2", ex);
            }
        }
    }
}
