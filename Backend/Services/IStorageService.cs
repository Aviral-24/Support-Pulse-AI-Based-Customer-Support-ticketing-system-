using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public interface IStorageService
{
    // Day 3 ke existing methods (Controllers ke liye)
    Task<string> UploadFileAsync(IFormFile file, string folderName); 
    string GenerateSignedUrl(string fileUrl);
    
    // Day 4 ka naya method (AI Audio ke liye)
    Task<Stream> GetFileStreamAsync(string fileName); 
}