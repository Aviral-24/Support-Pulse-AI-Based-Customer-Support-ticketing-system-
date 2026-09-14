// using System;
// using System.IO;
// using System.Threading.Tasks;
// using Amazon.S3;
// using Amazon.S3.Model;
// using Amazon.S3.Util;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Configuration;

// namespace Backend.Services;

// //public class S3StorageService : IStorageService
// public class AwsS3StorageService : IStorageService
// {
//     private readonly IAmazonS3 _s3Client;
//     private readonly string _bucketName;

//     //public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
//     public AwsS3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
//     {
//         _s3Client = s3Client;
//         _bucketName = configuration["AWS:BucketName"] ?? "support-pulse-files";
//     }

//     // // 1. Controller ke liye Upload method
//     // public async Task<string> UploadFileAsync(IFormFile file, string folderName)
//     // {
//     //     var fileName = $"{folderName}/{Guid.NewGuid()}_{file.FileName}";
        
//     //     using var stream = file.OpenReadStream();
//     //     var request = new PutObjectRequest
//     //     {
//     //         BucketName = _bucketName,
//     //         Key = fileName,
//     //         InputStream = stream,
//     //         ContentType = file.ContentType
//     //     };

//     //     await _s3Client.PutObjectAsync(request);
//     //     return fileName; 
//     // }

//     // 1. UPDATED: Auto-create bucket logic added
//     public async Task<string> UploadFileAsync(IFormFile file, string folderName)
//     {
//         // 🌟 Check if bucket exists, if not create it automatically
//         bool bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
//         if (!bucketExists)
//         {
//             var putBucketRequest = new PutBucketRequest
//             {
//                 BucketName = _bucketName,
//                 UseClientRegion = true
//             };
//             await _s3Client.PutBucketAsync(putBucketRequest);
//         }

//         var fileName = $"{folderName}/{Guid.NewGuid()}_{file.FileName}";
        
//         using var stream = file.OpenReadStream();
//         var request = new PutObjectRequest
//         {
//             BucketName = _bucketName,
//             Key = fileName,
//             InputStream = stream,
//             ContentType = file.ContentType
//         };

//         await _s3Client.PutObjectAsync(request);
//         return fileName; 
//     }

//     // 2. Controller ke liye Signed URL method
//     public string GenerateSignedUrl(string fileUrl)
//     {
//         if (string.IsNullOrEmpty(fileUrl)) return string.Empty;

//         var request = new GetPreSignedUrlRequest
//         {
//             BucketName = _bucketName,
//             Key = fileUrl,
//             Expires = DateTime.UtcNow.AddHours(2) 
//         };

//         return _s3Client.GetPreSignedURL(request);
//     }

//     // 3. Worker ke liye Stream method (Audio Transcription)
//     public async Task<Stream> GetFileStreamAsync(string fileName)
//     {
//         try
//         {
//             var key = Path.GetFileName(fileName); 
//             var request = new GetObjectRequest
//             {
//                 BucketName = _bucketName,
//                 Key = key
//             };
            
//             var response = await _s3Client.GetObjectAsync(request);
//             return response.ResponseStream; 
//         }
//         catch (AmazonS3Exception ex)
//         {
//             throw new Exception($"S3/MinIO error: {ex.Message}");
//         }
//     }
// }


using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public class AwsS3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    
    // File Validation Rules
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".mp3", ".wav" };
    private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB limit

    public AwsS3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"] ?? "support-pulse-files";
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        // 1. File Validation
        ValidateFile(file);

        // 2. Auto-create bucket with Retention Policy
        bool bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
        if (!bucketExists)
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = _bucketName,
                UseClientRegion = true
            };
            await _s3Client.PutBucketAsync(putBucketRequest);

            // Apply 30-Day Retention Policy (Lifecycle Rule)
            await ApplyRetentionPolicyAsync();
        }

        // 3. Upload File
        var fileName = $"{folderName}/{Guid.NewGuid()}_{file.FileName}";
        
        using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = stream,
            ContentType = file.ContentType
        };

        await _s3Client.PutObjectAsync(request);
        return fileName; 
    }

    public string GenerateSignedUrl(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return string.Empty;

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileUrl,
            Expires = DateTime.UtcNow.AddHours(2) 
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public async Task<Stream> GetFileStreamAsync(string fileName)
    {
        try
        {
            var key = Path.GetFileName(fileName); 
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };
            
            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream; 
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"S3/MinIO error: {ex.Message}");
        }
    }

    // Helper: Validates File Size and Extension
    private void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxFileSizeInBytes)
            throw new ArgumentException($"File size exceeds the {MaxFileSizeInBytes / (1024 * 1024)}MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException($"Invalid file type. Allowed types: {string.Join(", ", _allowedExtensions)}");
    }

    // Helper: Sets bucket lifecycle rule to delete files after 30 days
    private async Task ApplyRetentionPolicyAsync()
    {
        var lifecycleConfig = new LifecycleConfiguration
        {
            Rules = new List<LifecycleRule>
            {
                new LifecycleRule
                {
                    Id = "AutoDeleteAfter30Days",
                    Filter = new LifecycleFilter(), // Applies to all objects in the bucket
                    Status = LifecycleRuleStatus.Enabled,
                    Expiration = new LifecycleRuleExpiration { Days = 30 } 
                }
            }
        };

        var request = new PutLifecycleConfigurationRequest
        {
            BucketName = _bucketName,
            Configuration = lifecycleConfig
        };

        await _s3Client.PutLifecycleConfigurationAsync(request);
    }
}