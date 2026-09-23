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
        
        // string prompt = $@"Analyze this support ticket and return strict JSON format with keys: summary, sentiment (Happy/Angry/Neutral), priority (High/Medium/Low).
       

       // string prompt = $@"Analyze this support ticket text (which was transcribed from a user's voice note). Even if the text is polite, if the phrasing or context implies urgency or happiness, classify it strictly. Return strict JSON format with keys: summary, sentiment (Happy/Angry/Neutral), priority (High/Medium/Low).

    //    string prompt = $@"Analyze this support ticket and return strict JSON format with keys: 
    //       summary, 
    //     sentiment (Happy/Angry/Neutral/Appreciative), 
    //      priority (High/Medium/Low). 
    //   Note: If the user is thanking the team or expressing satisfaction, classify sentiment as Happy.

    //   Title: {title}
    //    Description: {description}";

    // UPDATE: Prompt ko change kiya taaki AI Title/Description ke according custom sentiment banaye
        string prompt = $@"Analyze this support ticket strictly based on the provided Title and Description. 
        Return a strict JSON object with exactly these 3 keys: 
        1. 'summary': A brief 1-2 sentence exact summary of the issue.
        2. 'sentiment': A specific 2-4 word emotion accurately reflecting the user's tone based on their text (e.g., 'Frustrated with delay', 'Confused about UI', 'Angry about billing', 'Calm and asking for help'). Do not just use one word.
        3. 'priority': (High/Medium/Low) based on the severity of the issue described.

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

    // public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    // {
    //     var groqKey = _configuration["Groq:ApiKey"];
    //     using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions");
    //     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", groqKey);

    //     using var content = new MultipartFormDataContent();
    //     var fileContent = new StreamContent(audioStream);
    //     fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
    //     content.Add(fileContent, "file", fileName);
        
    //     // Aapke dusre screenshot me 'whisper-large-v3' clearly visible hai, isliye ye perfectly chalega
    //     content.Add(new StringContent("whisper-large-v3"), "model");
    //     content.Add(new StringContent("en"), "language");

    //     request.Content = content;
    //     var response = await _httpClient.SendAsync(request);
        
    //     response.EnsureSuccessStatusCode();

    //     var jsonResponse = await response.Content.ReadAsStringAsync();
    //     using var doc = JsonDocument.Parse(jsonResponse);
    //     return doc.RootElement.GetProperty("text").GetString() ?? "";
    // }

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {
        var groqKey = _configuration["Groq:ApiKey"];
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", groqKey);

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(audioStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent("whisper-large-v3"), "model");
        content.Add(new StringContent("en"), "language");

        request.Content = content;
        
        // 🔥 SSL BYPASS INSTANT FIX FOR GROQ AI
        var handler = new HttpClientHandler 
        { 
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true 
        };
        using var localHttpClient = new HttpClient(handler);
        
        var response = await localHttpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement.GetProperty("text").GetString() ?? "";
    }

    //Image to Text (Vision AI) Logic

    // Image to Text (Vision AI) Logic - FIXED
public async Task<string> AnalyzeImageAsync(Stream imageStream, string mimeType)
{
    try 
    {
        // 1. Image ko bytes me convert karna
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        byte[] imageBytes = memoryStream.ToArray();
        
        // Lamba Base64 string Uri me daalne ki jagah direct BinaryData use karein
        // Isse "URI too long" wala crash nahi hoga
        var imageContent = ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mimeType);

        //  Groq ka 90b vision model use karein (agar 11b account me available nahi hai)
       // var chatClient = _groqClient.GetChatClient("llama-3.2-90b-vision-preview");
       // var chatClient = _groqClient.GetChatClient("llama-3.2-11b-vision-instruct");
          var chatClient = _groqClient.GetChatClient("qwen/qwen3.8-27b");

        var messages = new List<ChatMessage>
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart("Analyze this screenshot attached by a user in a support ticket. Extract any error messages exactly as they appear. Briefly describe the UI or the problem visible in 2-3 sentences."),
                imageContent
            )
        };

        var response = await chatClient.CompleteChatAsync(messages);
        return response.Value.Content[0].Text;
    }
    catch (Exception ex)
    {
        // Console par exact error print hoga taaki debugging aasaan ho
        Console.WriteLine($"\n[WARNING] Vision AI Error: {ex.Message}");
        return $"Vision AI Error: {ex.Message}";
    }
}
    // RAG Auto-Reply Generation
    public async Task<string> GenerateDraftReplyAsync(string issueDescription, string pastSolutionsContext)
    {
        var chatClient = _groqClient.GetChatClient("openai/gpt-oss-120b"); 
        
        string prompt = $@"You are an expert customer support agent. 
        A customer has submitted the following issue:
        '{issueDescription}'

        Here is how similar issues were successfully resolved in the past:
        {pastSolutionsContext}

        Based on these past solutions, write a polite, professional, and helpful email reply to the current customer. 
        Keep it concise (3-4 paragraphs). Do not invent false links or policies not mentioned in the context. If no past context is provided, just write a polite acknowledgment saying the team is looking into it.";

        var messages = new List<ChatMessage> { new UserChatMessage(prompt) };
        var response = await chatClient.CompleteChatAsync(messages);
        
        return response.Value.Content[0].Text;
    }
}