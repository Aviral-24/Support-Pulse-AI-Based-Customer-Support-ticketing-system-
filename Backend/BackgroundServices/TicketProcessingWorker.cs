using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Backend.Data; 
using Backend.Services;
using Backend.Services.Async; 
using Backend.Interfaces;
using Backend.Models;
using Backend.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace Backend.BackgroundServices;

public class TicketProcessingWorker : BackgroundService
{
    private readonly ITicketQueue _ticketQueue;
    private readonly IServiceScopeFactory _scopeFactory;

    public TicketProcessingWorker(ITicketQueue ticketQueue, IServiceScopeFactory scopeFactory)
    {
        _ticketQueue = ticketQueue;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🚀 AI Background Worker is running...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var ticketId = await _ticketQueue.DequeueTicketAsync(stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); 
                var _aiService = scope.ServiceProvider.GetRequiredService<IAIService>();
                var _storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
                var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                //  Customer details DB se fetch karne ke liye .Include() lagaya gaya
                var ticket = await _context.Tickets
                    .Include(t => t.Customer)
                    .FirstOrDefaultAsync(t => t.Id == ticketId, stoppingToken);

                if (ticket == null) continue;

                Console.WriteLine($"[PROCESSING] AI is analyzing Ticket #{ticketId} in background...");
                
                string finalDescription = ticket.Description ?? "";

                // AUDIO TRANSCRIPTION (CRASH-PROOF)
                if (!string.IsNullOrEmpty(ticket.AudioUrl))
                {
                    try 
                    {
                        var audioDownloadUrl = _storageService.GenerateSignedUrl(ticket.AudioUrl);
                        audioDownloadUrl = audioDownloadUrl.Replace("localhost", "support-pulse-s3-minio-1")
                                                           .Replace("127.0.0.1", "support-pulse-s3-minio-1")
                                                           .Replace("https://", "http://"); 

                        var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
                        using var httpClient = new HttpClient(handler);
                        
                        var audioBytes = await httpClient.GetByteArrayAsync(audioDownloadUrl, stoppingToken);
                        using var audioStream = new MemoryStream(audioBytes);

                        string transcript = await _aiService.TranscribeAudioAsync(audioStream, "audio.wav");
                        finalDescription = $"{finalDescription}\n\n[🎙️ Audio Transcript]: {transcript}";
                    }
                    catch (Exception audioEx)
                    {
                        finalDescription = $"{finalDescription}\n\n[⚠️ Audio Error]: {audioEx.Message}";
                    }
                }
                //  IMAGE VISION LOGIC (Auto-Pilot me add kiya)
                if (!string.IsNullOrEmpty(ticket.ImageUrl))
                {
                    try 
                    {
                        Console.WriteLine($"[VISION] Analyzing attached screenshot for Ticket #{ticketId}...");
                        
                        var imageDownloadUrl = _storageService.GenerateSignedUrl(ticket.ImageUrl);
                        imageDownloadUrl = imageDownloadUrl.Replace("localhost", "support-pulse-s3-minio-1")
                                                           .Replace("127.0.0.1", "support-pulse-s3-minio-1")
                                                           .Replace("https://", "http://"); 

                        var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
                        using var httpClient = new HttpClient(handler);
                        
                        var imageBytes = await httpClient.GetByteArrayAsync(imageDownloadUrl, stoppingToken);
                        using var imageStream = new MemoryStream(imageBytes);

                        // Image ka type guess karna (default png)
                        string mimeType = ticket.ImageUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                                          ticket.ImageUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) 
                                          ? "image/jpeg" : "image/png";

                        // AI ko call karna
                        string visionAnalysis = await _aiService.AnalyzeImageAsync(imageStream, mimeType);
                        
                        // Result ko final description me jod dena
                        finalDescription = $"{finalDescription}\n\n[🖼️ AI Image Analysis]: {visionAnalysis}";
                    }
                    catch (Exception imgEx)
                    {
                        finalDescription = $"{finalDescription}\n\n[⚠️ Image Error]: {imgEx.Message}";
                    }
                }

                ticket.Description = finalDescription; 

               //  TEXT ANALYTICS (Category, Summary, Sentiment, Priority)
              var aiResult = await _aiService.AnalyzeTicketAsync(ticket.Title ?? "", finalDescription);
             ticket.AiCategory = aiResult.Category;  
             ticket.AiSummary = aiResult.Summary;
             ticket.AiSentiment = aiResult.Sentiment;
             ticket.Priority = aiResult.Priority;    

                //  VECTOR EMBEDDINGS
                string textToEmbed = $"Title: {ticket.Title}. Details: {finalDescription}. Sentiment: {ticket.AiSentiment}";
                ticket.Embedding = await _aiService.GenerateEmbeddingAsync(textToEmbed);

                await _context.SaveChangesAsync(stoppingToken);
                
                Console.WriteLine($"[SUCCESS] Ticket #{ticketId} AI Enrichment Completed!");

                // Safely check for Malicious activity
                bool isMalicious = (!string.IsNullOrEmpty(ticket.AiCategory) && ticket.AiCategory.Contains("Security", StringComparison.OrdinalIgnoreCase)) || 
                                   (!string.IsNullOrEmpty(ticket.AiSentiment) && ticket.AiSentiment.Contains("Malicious", StringComparison.OrdinalIgnoreCase));

                // Safely check for Negative sentiment
                bool isNegative = (!string.IsNullOrEmpty(ticket.AiSentiment) && ticket.AiSentiment.Contains("Frustrated", StringComparison.OrdinalIgnoreCase)) || 
                                  (!string.IsNullOrEmpty(ticket.AiSentiment) && ticket.AiSentiment.Contains("Angry", StringComparison.OrdinalIgnoreCase));

                // Sirf Malicious ya Negative hone par hi alert bheja jayega
                if (isMalicious || isNegative)
                {
                    try
                    {
                        string subject = isMalicious ? $"🚨 CRITICAL SECURITY ALERT: Malicious Ticket #{ticket.Id}" 
                                                     : $"⚠️ URGENT: Negative Customer Sentiment on Ticket #{ticket.Id}";
                                                     
                        // Email Body me Customer Name aur Email add kiya gaya
                        string body = $@"
                            <h2>Alert for Support Ticket #{ticket.Id}</h2>
                            <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin-bottom: 15px;'>
                                <p style='margin: 0; font-size: 16px;'><strong>Customer Name:</strong> {ticket.Customer?.Name ?? "Unknown User"}</p>
                                <p style='margin: 0; font-size: 16px;'><strong>Customer Email:</strong> {ticket.Customer?.Email ?? "No Email Provided"}</p>
                            </div>
                            <p><strong>Title:</strong> {ticket.Title}</p>
                            <p><strong>Category:</strong> {ticket.AiCategory}</p>
                            <p><strong>Sentiment:</strong> <span style='color:red; font-weight:bold;'>{ticket.AiSentiment}</span></p>
                            <p><strong>AI Summary:</strong> {ticket.AiSummary}</p>
                            <br/>
                            <p>Please check the Agent Dashboard immediately to handle this issue.</p>";

                        // Admin email set karein
                        await _emailService.SendEmailAsync("aviral210462@acropolis.in", subject, body);
                        
                        Console.WriteLine($"[ALERT] Alert Email sent successfully for Ticket #{ticket.Id} (Reason: {(isMalicious ? "Malicious" : "Negative Sentiment")})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to send alert email in worker: {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AI Background Process Failed for Ticket #{ticketId}: {ex.Message}");
            }
        }
    }
}