using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using Backend.Services.Async; 
using Backend.Services;
using System.Security.Claims;
using Backend.Services.Pdf;
using Pgvector.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] 
public class TicketsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly ITicketQueue _ticketQueue; 
    
    public TicketsController(ApplicationDbContext context, IStorageService storageService, ITicketQueue ticketQueue) 
    {
        _context = context;
        _storageService = storageService;
        _ticketQueue = ticketQueue; 
    }

    [HttpPost]
    [Authorize(Roles = "Customer")] 
    public async Task<IActionResult> CreateTicket([FromForm] CreateTicketDto request)
    {
        var customerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string? audioUrl = null;
        string? imageUrl = null;

        try
        {
            if (request.AudioFile != null)
                audioUrl = await _storageService.UploadFileAsync(request.AudioFile, "uploads/audio");

            if (request.ImageFile != null)
                imageUrl = await _storageService.UploadFileAsync(request.ImageFile, "uploads/images");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"File upload failed: {ex.Message}" });
        }

        var ticket = new Ticket
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            CustomerId = customerId,
            AudioUrl = audioUrl,
            ImageUrl = imageUrl,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // 🔥 TICKET QUEUE ME JAA RAHI HAI
        await _ticketQueue.EnqueueTicketAsync(ticket.Id);
        
        return Ok(new { message = "Ticket created successfully! AI is analyzing it in the background.", ticketId = ticket.Id });
    }

    // 🔥 NEW: Customer ki khud ki tickets fetch karne ke liye
    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyTickets()
    {
        // Token se directly Customer ID nikal li (Secure BOLA protection)
        var customerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        
        var tickets = await _context.Tickets
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Category,
                t.CreatedAt,
                t.AiSummary // Agar AI ne summary banayi hai toh customer ko bhi dikhayenge
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("{id}")]

    [Authorize(Roles = "Admin")] // 🔥 Agent hata diya
    public async Task<IActionResult> GetTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        var response = new 
        {
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ImageUrl = _storageService.GenerateSignedUrl(ticket.ImageUrl!), 
            AudioUrl = _storageService.GenerateSignedUrl(ticket.AudioUrl!)
        };
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> GetTickets([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Tickets.Include(t => t.Customer).AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status.ToLower() == status.ToLower());

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

        var totalTickets = await query.CountAsync();
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new {
             t.Id, t.Title, t.Description, t.AudioUrl, t.ImageUrl,
             t.Status, t.Category, CustomerName = t.Customer!.Name,
             t.AiSummary, t.AiSentiment, t.CreatedAt 
            })
            .ToListAsync();

        return Ok(new { totalTickets, page, pageSize, tickets });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto request, [FromServices] IAuditService _auditService)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound("Ticket not found.");

        var validStatuses = new[] { "Open", "In Progress", "Resolved" };
        if (!validStatuses.Contains(request.Status)) return BadRequest("Invalid status.");
         
        ticket.Status = request.Status;
        await _context.SaveChangesAsync();

        var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _auditService.LogActionAsync(agentId, $"Changed status to {request.Status}", "Ticket", id, "Status updated via Agent Dashboard");

        return Ok(new { message = $"Ticket status updated to {request.Status}." });
    }
    
    [HttpPost("{id}/notes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddNote(int id, [FromBody] AddNoteDto request)
    {
        var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == id);
        if (!ticketExists) return NotFound("Ticket not found.");

        var note = new TicketNote { TicketId = id, AgentId = agentId, Note = request.Note };
        _context.TicketNotes.Add(note);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Note added successfully!" });
    }
   
    [HttpGet("{id}/pdf")]
    [Authorize]
    public async Task<IActionResult> DownloadTicketPdf(int id, [FromServices] ITicketPdfGenerator _pdfGenerator, [FromServices] IAuditService _auditService)
    {
        var ticket = await _context.Tickets.Include(t => t.Customer).FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null) return NotFound("Ticket not found.");

        try 
        {
            var pdfBytes = _pdfGenerator.GenerateTicketSummary(ticket);
            var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _auditService.LogActionAsync(agentId, "Downloaded PDF Summary", "Ticket", id, "PDF generated containing AI Insights");

            return File(pdfBytes, "application/pdf", $"Ticket_{ticket.Id}_Summary.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"PDF Generation failed: {ex.Message}");
        }
    }

    [HttpGet("semantic-search")]
    [Authorize] 
    public async Task<IActionResult> SemanticSearch([FromQuery] string query, [FromServices] IAIService _aiService)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Search query cannot be empty.");

        try
        {
            var queryVector = await _aiService.GenerateEmbeddingAsync(query);
            var similarTickets = await _context.Tickets
                .Where(t => t.Embedding != null)
                .OrderBy(t => t.Embedding!.CosineDistance(queryVector))
                .Take(5)
                .Select(t => new {
                    t.Id, t.Title, t.AiCategory, t.AiSummary, t.Status,
                    Distance = t.Embedding!.CosineDistance(queryVector) 
                })
                .ToListAsync();

            return Ok(similarTickets);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Semantic search failed: {ex.Message}");
        }
    }

    // // 🔥 THE SILVER BULLET: Direct AI Test Endpoint
    // [HttpGet("{id}/force-ai-test")]
    // [Authorize] 
    // public async Task<IActionResult> ForceAiTest(int id, [FromServices] IAIService _aiService)
    // {
    //     var ticket = await _context.Tickets.FindAsync(id);
    //     if (ticket == null) return NotFound("Ticket not found.");

    //     try
    //     {
    //         // 1. Text Analytics (Groq)
    //         var aiResult = await _aiService.AnalyzeTicketAsync(ticket.Title ?? "", ticket.Description ?? "");
    //         ticket.AiSummary = aiResult.Summary;
    //         ticket.AiSentiment = aiResult.Sentiment;

    //         // 2. Vector Embeddings (HuggingFace)
    //         string textToEmbed = $"Title: {ticket.Title}. Details: {ticket.Description}. Sentiment: {ticket.AiSentiment}";
    //         ticket.Embedding = await _aiService.GenerateEmbeddingAsync(textToEmbed);

    //         ticket.Status = "In Progress";
    //         await _context.SaveChangesAsync(); // Database me vectors save!

    //         return Ok(new { 
    //             message = "🔥 AI Enrichment SUCCESSFUL! Vectors are saved to Database.", 
    //             summary = ticket.AiSummary, 
    //             sentiment = ticket.AiSentiment 
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, new { message = "AI API Failed", error = ex.Message });
    //     }
    // }

    // // 🔥 THE SILVER BULLET: Direct AI Test Endpoint (UPDATED FOR CASE 3)
    // [HttpGet("{id}/force-ai-test")]
    // [Authorize] 
    // public async Task<IActionResult> ForceAiTest(int id, [FromServices] IAIService _aiService, [FromServices] IStorageService _storageService)
    // {
    //     var ticket = await _context.Tickets.FindAsync(id);
    //     if (ticket == null) return NotFound("Ticket not found.");

    //     try
    //     {
    //         string finalDescription = ticket.Description ?? "";

    //         // // 1. 🔥 AUDIO TRANSCRIPTION LOGIC (CASE 3 FIX)
    //         // if (!string.IsNullOrEmpty(ticket.AudioUrl))
    //         // {
    //         //     // MinIO se file ka temporary URL nikal kar download karein
    //         //     var audioDownloadUrl = _storageService.GenerateSignedUrl(ticket.AudioUrl);
    //         //     using var httpClient = new HttpClient();
    //         //     var audioBytes = await httpClient.GetByteArrayAsync(audioDownloadUrl);
    //         //     using var audioStream = new MemoryStream(audioBytes);

    //         //     // Whisper AI ko bhejein
    //         //     string transcript = await _aiService.TranscribeAudioAsync(audioStream, "audio.wav");

    //         //     // Transcript ko original description ke niche jod dein
    //         //     finalDescription = $"{finalDescription}\n\n[🎙️ Audio Transcript]: {transcript}";
    //         //     ticket.Description = finalDescription; 
    //         // }

    //         // 1. 🔥 AUDIO TRANSCRIPTION LOGIC (CASE 3 FIX)
    //         if (!string.IsNullOrEmpty(ticket.AudioUrl))
    //         {
    //             var audioDownloadUrl = _storageService.GenerateSignedUrl(ticket.AudioUrl);
                
    //             // 🔥 DOCKER NETWORK FIX: Agar URL me 'localhost' hai toh Docker container ka actual naam use karein
    //             audioDownloadUrl = audioDownloadUrl.Replace("localhost", "support-pulse-s3-minio-1")
    //                                                .Replace("127.0.0.1", "support-pulse-s3-minio-1");

    //             // 🔥 SSL BYPASS FIX: Docker ke SSL errors ko ignore karne ke liye handler add kiya
    //             var handler = new HttpClientHandler 
    //             { 
    //                 ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true 
    //             };
    //             using var httpClient = new HttpClient(handler);
                
    //             var audioBytes = await httpClient.GetByteArrayAsync(audioDownloadUrl);
    //             using var audioStream = new MemoryStream(audioBytes);

    //             // Whisper AI ko bhejein
    //             string transcript = await _aiService.TranscribeAudioAsync(audioStream, "audio.wav");

    //             // Transcript ko original description ke niche jod dein
    //             finalDescription = $"{finalDescription}\n\n[🎙️ Audio Transcript]: {transcript}";
    //             ticket.Description = finalDescription; 
    //         }

    //         // 2. Text Analytics (Groq) - Ab ye audio transcript ko bhi padhega!
    //         var aiResult = await _aiService.AnalyzeTicketAsync(ticket.Title ?? "", finalDescription);
    //         ticket.AiSummary = aiResult.Summary;
    //         ticket.AiSentiment = aiResult.Sentiment;
    //         ticket.AiCategory = aiResult.Priority; // Priority save kar rahe hain

    //         // 3. Vector Embeddings (HuggingFace)
    //         string textToEmbed = $"Title: {ticket.Title}. Details: {finalDescription}. Sentiment: {ticket.AiSentiment}";
    //         ticket.Embedding = await _aiService.GenerateEmbeddingAsync(textToEmbed);

    //         ticket.Status = "In Progress";
    //         await _context.SaveChangesAsync(); 

    //         return Ok(new { 
    //             message = "🔥 Audio Transcription & AI Enrichment SUCCESSFUL!", 
    //             transcriptAdded = !string.IsNullOrEmpty(ticket.AudioUrl),
    //             updatedDescription = ticket.Description,
    //             summary = ticket.AiSummary, 
    //             sentiment = ticket.AiSentiment 
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, new { message = "AI API Failed", error = ex.Message });
    //     }
    // }


// 🔥 THE SILVER BULLET: Direct AI Test Endpoint (CRASH-PROOF VERSION)
    [HttpGet("{id}/force-ai-test")]
    [Authorize] 
    public async Task<IActionResult> ForceAiTest(int id, [FromServices] IAIService _aiService, [FromServices] IStorageService _storageService)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound("Ticket not found.");

        try
        {
            string finalDescription = ticket.Description ?? "";

            // 1. 🔥 AUDIO TRANSCRIPTION LOGIC (CASE 3 FIX)
            if (!string.IsNullOrEmpty(ticket.AudioUrl))
            {
                try 
                {
                    var audioDownloadUrl = _storageService.GenerateSignedUrl(ticket.AudioUrl);
                    
                    // 🔥 MAIN FIX: Container name lagaya aur FORCEFULLY 'https' ko 'http' kiya
                    audioDownloadUrl = audioDownloadUrl.Replace("localhost", "support-pulse-s3-minio-1")
                                                       .Replace("127.0.0.1", "support-pulse-s3-minio-1")
                                                       .Replace("https://", "http://"); 

                    var handler = new HttpClientHandler 
                    { 
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true 
                    };
                    using var httpClient = new HttpClient(handler);
                    
                    var audioBytes = await httpClient.GetByteArrayAsync(audioDownloadUrl);
                    using var audioStream = new MemoryStream(audioBytes);

                    // Whisper AI ko bhejein
                    string transcript = await _aiService.TranscribeAudioAsync(audioStream, "audio.wav");
                    
                    finalDescription = $"{finalDescription}\n\n[🎙️ Audio Transcript]: {transcript}";
                }
                catch (Exception audioEx)
                {
                    // 🛡️ SAFETY NET: Agar MinIO ya Whisper fail ho, toh app crash nahi hogi!
                    finalDescription = $"{finalDescription}\n\n[⚠️ Audio Error]: {audioEx.Message}";
                }
            }

            ticket.Description = finalDescription; 

            // 2. Text Analytics (Groq)
            var aiResult = await _aiService.AnalyzeTicketAsync(ticket.Title ?? "", finalDescription);
            ticket.AiSummary = aiResult.Summary;
            ticket.AiSentiment = aiResult.Sentiment;
            ticket.AiCategory = aiResult.Priority;

            // 3. Vector Embeddings (HuggingFace)
            string textToEmbed = $"Title: {ticket.Title}. Details: {finalDescription}. Sentiment: {ticket.AiSentiment}";
            ticket.Embedding = await _aiService.GenerateEmbeddingAsync(textToEmbed);

            ticket.Status = "In Progress";
            await _context.SaveChangesAsync(); 

            return Ok(new { 
                message = "🔥 AI Enrichment Processed Successfully!", 
                transcriptAdded = !string.IsNullOrEmpty(ticket.AudioUrl),
                updatedDescription = ticket.Description,
                summary = ticket.AiSummary, 
                sentiment = ticket.AiSentiment 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "AI API Failed", error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    // 🔥 THE RAG ENDPOINT: Auto-Generate Reply based on Vector DB
    [HttpPost("{id}/draft-reply")]
    [Authorize]
    public async Task<IActionResult> GenerateDraftReply(int id, [FromServices] IAIService _aiService)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound("Ticket not found.");
        if (ticket.Embedding == null) return BadRequest("Ticket is still being analyzed by AI. Please wait.");

        try
        {
            // 1. RETRIEVE: Vector DB me purani RESOLVED tickets dhoondo jo is issue se milti-julti hon
            var similarResolvedTickets = await _context.Tickets
                .Where(t => t.Id != id && t.Status == "Resolved" && t.Embedding != null)
                .OrderBy(t => t.Embedding!.L2Distance(ticket.Embedding)) // PgVector Semantic Search
                .Take(2) // Top 2 sabse accurate solutions
                .ToListAsync();

            // 2. AUGMENT: Un solutions ka text combine karo
            string context = "";
            foreach (var t in similarResolvedTickets)
            {
                context += $"- Past Issue: {t.Title}\n- Details & Solution: {t.Description}\n\n";
            }

            if (string.IsNullOrEmpty(context)) {
                context = "No similar resolved past tickets found in the database.";
            }

            // 3. GENERATE: Groq AI ko context bhej kar email draft karwao
            string draft = await _aiService.GenerateDraftReplyAsync(ticket.Description ?? ticket.Title ?? "", context);

            return Ok(new { draftReply = draft });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to generate AI reply.", error = ex.Message });
        }
    }
}