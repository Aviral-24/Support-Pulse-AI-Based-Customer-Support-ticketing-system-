using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Backend.Data;
using Backend.Services;

namespace Backend.Services.Async;

public class AiEnrichmentWorker : BackgroundService
{
    private readonly ITicketQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AiEnrichmentWorker> _logger;

    public AiEnrichmentWorker(ITicketQueue queue, IServiceProvider serviceProvider, ILogger<AiEnrichmentWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AI Enrichment Worker is starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ticketId = await _queue.DequeueTicketAsync(stoppingToken);
                
                if (ticketId > 0)
                {
                    _logger.LogInformation($"Processing Ticket ID: {ticketId}");
                    await ProcessTicketAsync(ticketId);
                }
                else
                {
                    await Task.Delay(2000, stoppingToken); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Worker loop error: {ex.Message}");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessTicketAsync(int ticketId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();
        var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>(); 

        var ticket = await context.Tickets.FindAsync(ticketId);
        if (ticket == null) return;

        try
        {
            // 1. AUDIO TRANSCRIPTION FIX
            // Note: Agar aapke Ticket model me property ka naam AudioUrl nahi hai (jaise AudioFile hai), 
            // toh kripya usey yahan replace karein: ticket.AudioUrl ki jagah ticket.AudioFile
            if (!string.IsNullOrEmpty(ticket.AudioUrl)) 
            {
                _logger.LogInformation($"Downloading and transcribing audio for Ticket {ticketId}...");
                
                // Direct stream fetch karna (No MemoryStream needed!)
                var audioStream = await storageService.GetFileStreamAsync(ticket.AudioUrl); 
                
                var transcription = await aiService.TranscribeAudioAsync(audioStream, "audio.mp3");
                ticket.Description = $"{ticket.Description}\n\n[Transcript]: {transcription}";
            }

            // 2. TEXT ANALYTICS
            var aiResult = await aiService.AnalyzeTicketAsync(ticket.Title ?? "", ticket.Description ?? "");
            ticket.AiSummary = aiResult.Summary;
            ticket.AiSentiment = aiResult.Sentiment;
            
            // 🔥 NOTE: Agar aapke Ticket model me "Priority" property define NAHI hai, 
            // toh niche wali line error degi. Isliye ise by default comment rakha hai. 
            // Agar property hai toh isey uncomment kar lein:
            // ticket.Priority = aiResult.Priority; 

            // 3. VECTOR EMBEDDINGS
            string textToEmbed = $"Title: {ticket.Title}. Details: {ticket.Description}. Sentiment: {ticket.AiSentiment}";
            ticket.Embedding = await aiService.GenerateEmbeddingAsync(textToEmbed);

            ticket.Status = "In Progress";
            await context.SaveChangesAsync();

            _logger.LogInformation($"✅ Ticket {ticketId} successfully enriched with AI!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to enrich Ticket {ticketId}. Error: {ex.Message}");
        }
    }
}