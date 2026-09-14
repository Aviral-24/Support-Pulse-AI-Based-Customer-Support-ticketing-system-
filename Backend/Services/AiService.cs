// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Net.Http.Json;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;
// using OpenAI;
// using OpenAI.Chat;
// using Pgvector;
// using System.ClientModel;

// namespace Backend.Services;

// public class AiService : IAIService
// {
//     private readonly OpenAIClient _groqClient;
//     private readonly HttpClient _httpClient; 
//     private readonly IConfiguration _configuration;

//     public AiService(IConfiguration configuration, HttpClient httpClient)
//     {
//         _configuration = configuration;
//         _httpClient = httpClient;
        
//         var groqApiKey = _configuration["Groq:ApiKey"] ?? "mock-api-key-for-testing";
//         var options = new OpenAIClientOptions 
//         { 
//             Endpoint = new Uri("https://api.groq.com/openai/v1") 
//         };
//         _groqClient = new OpenAIClient(new ApiKeyCredential(groqApiKey), options);

//         var hfApiKey = _configuration["HuggingFace:ApiKey"];
//         if (!string.IsNullOrEmpty(hfApiKey))
//         {
//             _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hfApiKey);
//         }
//     }

//     // 1. Text Analytics (Groq Llama 3.3)
//     public async Task<(string Summary, string Sentiment, string Priority)> AnalyzeTicketAsync(string title, string description)
//     {
//         try
//         {
//             if (_configuration["Groq:ApiKey"] == null)
//             {
//                 await Task.Delay(500); 
//                 return ("User reported a technical issue.", "Neutral", "Medium");
//             }

//             var chatClient = _groqClient.GetChatClient("llama-3.3-70b-versatile");
            
//             string prompt = $@"Analyze this support ticket and return strict JSON format with keys: summary, sentiment (Happy/Angry/Neutral), priority (High/Medium/Low).
//             Title: {title}
//             Description: {description}";

//             var chatOptions = new ChatCompletionOptions 
//             { 
//                 ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() 
//             };

//             var messages = new List<ChatMessage> { new UserChatMessage(prompt) };
//             var response = await chatClient.CompleteChatAsync(messages, chatOptions);
//             var content = response.Value.Content[0].Text;

//             using var jsonDoc = JsonDocument.Parse(content);
//             var root = jsonDoc.RootElement;
            
//             string summary = root.TryGetProperty("summary", out var sumProp) ? sumProp.GetString() ?? "No summary" : "No summary";
//             string sentiment = root.TryGetProperty("sentiment", out var sentProp) ? sentProp.GetString() ?? "Neutral" : "Neutral";
//             string priority = root.TryGetProperty("priority", out var prioProp) ? prioProp.GetString() ?? "Medium" : "Medium";

//             return (summary, sentiment, priority);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Groq AI Error: {ex.Message}");
//             return ("AI analysis temporarily unavailable.", "Neutral", "Medium");
//         }
//     }

//     // 2. Vector Embeddings (Hugging Face with Retry Logic)
//     public async Task<Vector> GenerateEmbeddingAsync(string text)
//     {
//         try
//         {
//             if (string.IsNullOrEmpty(_configuration["HuggingFace:ApiKey"])) return new Vector(new float[384]);

//             var payload = new { inputs = text };
//             string hfEndpoint = "https://api-inference.huggingface.co/pipeline/feature-extraction/sentence-transformers/all-MiniLM-L6-v2";
            
//             var response = await _httpClient.PostAsJsonAsync(hfEndpoint, payload);
            
//             // Handle Hugging Face "Cold Start" (503 Service Unavailable)
//             if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
//             {
//                 Console.WriteLine("HuggingFace model is loading. Waiting 5 seconds and retrying...");
//                 await Task.Delay(5000); 
//                 response = await _httpClient.PostAsJsonAsync(hfEndpoint, payload); 
//             }

//             response.EnsureSuccessStatusCode();
//             var vectorFloatArray = await response.Content.ReadFromJsonAsync<float[]>();
            
//             return new Vector(vectorFloatArray!);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"HF Embedding Error: {ex.Message}");
//             return new Vector(new float[384]);
//         }
//     }

//     // 3. Audio Transcription (Groq Whisper-large-v3)
//     public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
//     {
//         try
//         {
//             var groqKey = _configuration["Groq:ApiKey"];
//             if (string.IsNullOrEmpty(groqKey)) return "Audio transcription skipped (No API Key).";

//             using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions");
//             request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", groqKey);

//             using var content = new MultipartFormDataContent();
//             var fileContent = new StreamContent(audioStream);
//             fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            
//             content.Add(fileContent, "file", fileName);
//             content.Add(new StringContent("whisper-large-v3"), "model");
//             content.Add(new StringContent("en"), "language");

//             request.Content = content;
//             var response = await _httpClient.SendAsync(request);
//             response.EnsureSuccessStatusCode();

//             var jsonResponse = await response.Content.ReadAsStringAsync();
//             using var doc = JsonDocument.Parse(jsonResponse);
//             return doc.RootElement.GetProperty("text").GetString() ?? "";
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Groq Whisper Error: {ex.Message}");
//             return "[Audio transcription failed due to API error]";
//         }
//     }
// }
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using Pgvector;
using System.ClientModel;

namespace Backend.Services;

public class AiService : IAIService
{
    private readonly OpenAIClient _groqClient;
    private readonly HttpClient _httpClient; 
    private readonly IConfiguration _configuration;

    public AiService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        
        var groqApiKey = _configuration["Groq:ApiKey"];
        var options = new OpenAIClientOptions { Endpoint = new Uri("https://api.groq.com/openai/v1") };
        
        // Agar API key nahi hai, toh turant fail hoga, fake analysis nahi dega
        _groqClient = new OpenAIClient(new ApiKeyCredential(groqApiKey ?? ""), options);

        var hfApiKey = _configuration["HuggingFace:ApiKey"];
        if (!string.IsNullOrEmpty(hfApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hfApiKey);
        }
    }

    public async Task<(string Summary, string Sentiment, string Priority)> AnalyzeTicketAsync(string title, string description)
    {
        // 🔥 UPDATE: Aapke screenshot ke according exact model name daal diya gaya hai
        var chatClient = _groqClient.GetChatClient("openai/gpt-oss-120b");
        
        string prompt = $@"Analyze this support ticket and return strict JSON format with keys: summary, sentiment (Happy/Angry/Neutral), priority (High/Medium/Low).
        Title: {title}
        Description: {description}";

        var chatOptions = new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() };
        var messages = new List<ChatMessage> { new UserChatMessage(prompt) };
        
        var response = await chatClient.CompleteChatAsync(messages, chatOptions);
        var content = response.Value.Content[0].Text;

        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        string summary = root.TryGetProperty("summary", out var sumProp) ? sumProp.GetString() ?? "No summary" : "No summary";
        string sentiment = root.TryGetProperty("sentiment", out var sentProp) ? sentProp.GetString() ?? "Neutral" : "Neutral";
        string priority = root.TryGetProperty("priority", out var prioProp) ? prioProp.GetString() ?? "Medium" : "Medium";

        return (summary, sentiment, priority);
    }

    // public async Task<Vector> GenerateEmbeddingAsync(string text)
    // {
    //     var payload = new { inputs = text };
    //     string hfEndpoint = "https://api-inference.huggingface.co/pipeline/feature-extraction/sentence-transformers/all-MiniLM-L6-v2";
        
    //     var response = await _httpClient.PostAsJsonAsync(hfEndpoint, payload);
        
    //     if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
    //     {
    //         await Task.Delay(5000); 
    //         response = await _httpClient.PostAsJsonAsync(hfEndpoint, payload); 
    //     }

    //     // Agar HF API fail hui toh ye error throw karega
    //     response.EnsureSuccessStatusCode(); 
        
    //     var vectorFloatArray = await response.Content.ReadFromJsonAsync<float[]>();
    //     return new Vector(vectorFloatArray!);
    // }

    // 2. Vector Embeddings (Hugging Face with Safe Fallback)
    public async Task<Vector> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var payload = new { inputs = text };
            string hfEndpoint = "https://api-inference.huggingface.co/pipeline/feature-extraction/sentence-transformers/all-MiniLM-L6-v2";
            
            var response = await _httpClient.PostAsJsonAsync(hfEndpoint, payload);
            
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                await Task.Delay(5000); 
                response = await _httpClient.PostAsJsonAsync(hfEndpoint, payload); 
            }

            response.EnsureSuccessStatusCode(); 
            var vectorFloatArray = await response.Content.ReadFromJsonAsync<float[]>();
            return new Vector(vectorFloatArray!);
        }
        catch (Exception ex)
        {
            // 🔥 Agar ISP/Wi-Fi HuggingFace ko block kare, toh server crash hone se bachayega
            Console.WriteLine($"\n[WARNING] HuggingFace Network Blocked: {ex.Message}");
            Console.WriteLine("Generating a safe fallback vector to prevent application crash...");
            
            // Safe fallback vector (PgVector me DivideByZero error rokne ke liye Index 0 ko 1 set kiya hai)
            float[] fallback = new float[384];
            fallback[0] = 1.0f; 
            return new Vector(fallback);
        }
    }

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {
        var groqKey = _configuration["Groq:ApiKey"];
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", groqKey);

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(audioStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        content.Add(fileContent, "file", fileName);
        
        // Aapke dusre screenshot me 'whisper-large-v3' clearly visible hai, isliye ye perfectly chalega
        content.Add(new StringContent("whisper-large-v3"), "model");
        content.Add(new StringContent("en"), "language");

        request.Content = content;
        var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement.GetProperty("text").GetString() ?? "";
    }
}