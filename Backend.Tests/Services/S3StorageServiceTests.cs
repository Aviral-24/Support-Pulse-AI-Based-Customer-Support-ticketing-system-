using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Backend.Services;

namespace Backend.Tests.Services
{
    public class AwsS3StorageServiceTests
    {
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AwsS3StorageService _storageService;

        public AwsS3StorageServiceTests()
        {
            // 🟢 ARRANGE: Mocking AWS S3 Client & Config
            _mockS3Client = new Mock<IAmazonS3>();
            
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["AWS:BucketName"]).Returns("test-bucket");

            _storageService = new AwsS3StorageService(_mockS3Client.Object, _mockConfig.Object);
        }

        // Helper: Fake File Banane Ke Liye
        private Mock<IFormFile> CreateFakeFile(string fileName, long length)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(length);
            return mockFile;
        }

        [Fact]
        public async Task UploadFileAsync_EmptyFile_ThrowsArgumentException()
        {
            // 🟢 ARRANGE
            var fakeFile = CreateFakeFile("test.jpg", length: 0); // Empty file

            // 🟢 ACT & ASSERT
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _storageService.UploadFileAsync(fakeFile.Object, "uploads"));

            Assert.Equal("File is empty.", exception.Message);
        }

        [Fact]
        public async Task UploadFileAsync_FileTooLarge_ThrowsArgumentException()
        {
            // 🟢 ARRANGE
            long sixMegabytes = 6 * 1024 * 1024; // 6 MB (Limit is 5 MB)
            var fakeFile = CreateFakeFile("large.png", length: sixMegabytes);

            // 🟢 ACT & ASSERT
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _storageService.UploadFileAsync(fakeFile.Object, "uploads"));

            Assert.Contains("File size exceeds", exception.Message);
        }

        [Fact]
        public async Task UploadFileAsync_InvalidExtension_ThrowsArgumentException()
        {
            // 🟢 ARRANGE
            var fakeFile = CreateFakeFile("virus.exe", length: 1024); // .exe is not allowed

            // 🟢 ACT & ASSERT
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _storageService.UploadFileAsync(fakeFile.Object, "uploads"));

            Assert.Contains("Invalid file type", exception.Message);
        }

        [Fact]
        public void GenerateSignedUrl_ValidKey_ReturnsPreSignedUrl()
        {
            // 🟢 ARRANGE
            string testKey = "uploads/test-image.jpg";
            string expectedUrl = "https://test-bucket.s3.amazonaws.com/uploads/test-image.jpg?signature=fake";

            _mockS3Client.Setup(s3 => s3.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
                         .Returns(expectedUrl);

            // 🟢 ACT
            var result = _storageService.GenerateSignedUrl(testKey);

            // 🟢 ASSERT
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void GenerateSignedUrl_EmptyKey_ReturnsEmptyString()
        {
            // 🟢 ACT
            var result = _storageService.GenerateSignedUrl("");

            // 🟢 ASSERT
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetFileStreamAsync_ValidFileName_ReturnsStream()
        {
            // 🟢 ARRANGE
            string fileName = "test.pdf";
            var fakeStream = new MemoryStream();
            
            var mockResponse = new GetObjectResponse { ResponseStream = fakeStream };
            
            _mockS3Client.Setup(s3 => s3.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mockResponse);

            // 🟢 ACT
            var result = await _storageService.GetFileStreamAsync(fileName);

            // 🟢 ASSERT
            Assert.NotNull(result);
            Assert.Same(fakeStream, result);
        }
    }
}