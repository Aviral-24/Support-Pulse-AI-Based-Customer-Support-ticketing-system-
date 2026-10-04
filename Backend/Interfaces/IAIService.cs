using System.IO;
using System.Threading.Tasks;
using Pgvector;

namespace Backend.Interfaces;

public interface IAIService
{
    // Text Analytics
Task<(string Category, string Summary, string Sentiment, string Priority)> AnalyzeTicketAsync(string title, string description);    
    // Vector Embedding
    Task<Vector> GenerateEmbeddingAsync(string text);
    
    // Audio Transcription
    Task<string> TranscribeAudioAsync(Stream audioStream, string fileName); 

    //  NEW: Vision AI Method
    Task<string> AnalyzeImageAsync(Stream imageStream, string mimeType);

    Task<string> GenerateDraftReplyAsync(string issueDescription, string pastSolutionsContext);
}