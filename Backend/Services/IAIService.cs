using System.IO;
using System.Threading.Tasks;
using Pgvector;

namespace Backend.Services;

public interface IAIService
{
    // Text Analytics
    Task<(string Summary, string Sentiment, string Priority)> AnalyzeTicketAsync(string title, string description);
    
    // Vector Embedding
    Task<Vector> GenerateEmbeddingAsync(string text);
    
    // Audio Transcription
    Task<string> TranscribeAudioAsync(Stream audioStream, string fileName); 
}