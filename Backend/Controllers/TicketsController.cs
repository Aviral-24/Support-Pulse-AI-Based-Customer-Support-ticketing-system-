using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using Backend.Services.Async; 
using Backend.Services;
using Backend.Services.Email; 
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

        // TICKET QUEUE ME JAA RAHI HAI
        await _ticketQueue.EnqueueTicketAsync(ticket.Id);
        
        return Ok(new { message = "Ticket created successfully! AI is analyzing it in the background.", ticketId = ticket.Id });
    }

    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyTickets()
    {
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
                t.AiSummary, 
                t.AiTranscription 
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Agent")] 
    public async Task<IActionResult> GetTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        var response = new 
        {
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.AiTranscription, 
            ticket.AiSummary,
            ticket.AiSentiment,
            ticket.RagDraftReply,
            ImageUrl = !string.IsNullOrEmpty(ticket.ImageUrl) ? _storageService.GenerateSignedUrl(ticket.ImageUrl) : null, 
            AudioUrl = !string.IsNullOrEmpty(ticket.AudioUrl) ? _storageService.GenerateSignedUrl(ticket.AudioUrl) : null
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
             t.AiSummary, t.AiSentiment, t.AiTranscription, 
             t.CreatedAt 
            })
            .ToListAsync();

        return Ok(new { totalTickets, page, pageSize, tickets });
    }

    // [HttpPut("{id}/status")]
    // [Authorize(Roles = "Admin,Agent")]
    // public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto request, [FromServices] IAuditService _auditService)
    // {
    //     var ticket = await _context.Tickets.FindAsync(id);
    //     if (ticket == null) return NotFound("Ticket not found.");

    //     var validStatuses = new[] { "Open", "In Progress", "Resolved" };
    //     if (!validStatuses.Contains(request.Status)) return BadRequest("Invalid status.");
         
    //     ticket.Status = request.Status;
    //     await _context.SaveChangesAsync();

    //     var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    //     await _auditService.LogActionAsync(agentId, $"Changed status to {request.Status}", "Ticket", id, "Status updated via Agent Dashboard");

    //     return Ok(new { message = $"Ticket status updated to {request.Status}." });
    // }

     // Phase 7 (Concurrency & Race Condition)
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto request, [FromServices] IAuditService _auditService)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound("Ticket not found.");

        var validStatuses = new[] { "Open", "In Progress", "Resolved" };
        if (!validStatuses.Contains(request.Status)) return BadRequest("Invalid status.");
         
        ticket.Status = request.Status;

        // 🚨 YAHAN CONCURRENCY LOCK LAGA HAI 🚨
        try
        {
            await _context.SaveChangesAsync();

            var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _auditService.LogActionAsync(agentId, $"Changed status to {request.Status}", "Ticket", id, "Status updated via Agent Dashboard");

            return Ok(new { message = $"Ticket status updated to {request.Status}." });
        }
        catch (DbUpdateConcurrencyException)
        {
            // Jab 10 log ek sath hit karenge, toh 9 logo ko ye 409 error milega
            return StatusCode(409, new { message = "Conflict: Yeh ticket already kisi aur ne update kar diya hai." });
        }
    }
    
    [HttpPost("{id}/notes")]
    [Authorize(Roles = "Admin,Agent")]
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
        var ticket = await _context.Tickets
        .Include(t => t.Customer)
        .Include(t => t.Notes)
        .FirstOrDefaultAsync(t => t.Id == id);
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

    // THE SILVER BULLET: Direct AI Test Endpoint
    [HttpGet("{id}/force-ai-test")]
    [Authorize] 
    public async Task<IActionResult> ForceAiTest(int id, [FromServices] IAIService _aiService, [FromServices] IStorageService _storageService, [FromServices] IEmailService _emailService)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound("Ticket not found.");

        try
        {
            string finalDescription = ticket.Description ?? "";

            // AUDIO TRANSCRIPTION LOGIC
            if (!string.IsNullOrEmpty(ticket.AudioUrl))
            {
                try 
                {
                    var audioDownloadUrl = _storageService.GenerateSignedUrl(ticket.AudioUrl);
                    
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

                    string transcript = await _aiService.TranscribeAudioAsync(audioStream, "audio.wav");
                    
                    ticket.AiTranscription = transcript; 
                    finalDescription = $"{finalDescription}\n\n[🎙️ Audio Transcript]: {transcript}";
                }
                catch (Exception audioEx)
                {
                    finalDescription = $"{finalDescription}\n\n[⚠️ Audio Error]: {audioEx.Message}";
                }
            }

            ticket.Description = finalDescription; 

            /// 2. Text Analytics (Groq)
            var aiResult = await _aiService.AnalyzeTicketAsync(ticket.Title ?? "", finalDescription);
           ticket.AiCategory = aiResult.Category;  
           ticket.AiSummary = aiResult.Summary;
           ticket.AiSentiment = aiResult.Sentiment;
           ticket.Priority = aiResult.Priority;  

          // 🚨 Updated Email Notification Trigger (Flexible Condition)
            string sentimentLower = aiResult.Sentiment.ToLower();
            
            if (sentimentLower.Contains("malicious") || sentimentLower.Contains("spam") || sentimentLower.Contains("negative") || sentimentLower.Contains("frustrated"))
            {
                ticket.Status = "Flagged"; 
                
                string adminEmail = "aviral210462@acropolis.in"; 
                string subject = $"🚨 SECURITY ALERT: Ticket Flagged (#{ticket.Id})";
                string body = $@"
                    <h2>Security & Sentiment Alert</h2>
                    <p>AI has flagged a ticket due to negative or malicious sentiment.</p>
                    <ul>
                        <li><strong>Ticket ID:</strong> {ticket.Id}</li>
                        <li><strong>Title:</strong> {ticket.Title}</li>
                        <li><strong>AI Sentiment:</strong> <span style='color:red;'>{aiResult.Sentiment}</span></li>
                        <li><strong>Summary:</strong> {aiResult.Summary}</li>
                    </ul>
                    <p>Please review this ticket immediately in the Agent Dashboard.</p>";

                // Fire and forget email dispatch
                _ = _emailService.SendEmailAsync(adminEmail, subject, body);
            }
            else 
            {
                ticket.Status = "In Progress";
            }

            // 3. Vector Embeddings (HuggingFace)
            string textToEmbed = $"Title: {ticket.Title}. Details: {finalDescription}. Sentiment: {ticket.AiSentiment}";
            ticket.Embedding = await _aiService.GenerateEmbeddingAsync(textToEmbed);

            await _context.SaveChangesAsync(); 

            return Ok(new { 
                message = "🔥 AI Enrichment Processed Successfully!", 
                transcriptAdded = !string.IsNullOrEmpty(ticket.AudioUrl),
                aiTranscription = ticket.AiTranscription, 
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

    [HttpPost("{id}/draft-reply")]
    [Authorize]
    public async Task<IActionResult> GenerateDraftReply(int id, [FromServices] IAIService _aiService)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound("Ticket not found.");
        if (ticket.Embedding == null) return BadRequest("Ticket is still being analyzed by AI. Please wait.");

        try
        {
            var relevantFaqs = await _context.Set<KnowledgeBase>()
                .Where(k => k.Embedding != null)
                .OrderBy(k => k.Embedding!.L2Distance(ticket.Embedding)) 
                .Take(2) 
                .ToListAsync();

            var similarResolvedTickets = await _context.Tickets
                .Where(t => t.Id != id && t.Status == "Resolved" && t.Embedding != null)
                .OrderBy(t => t.Embedding!.L2Distance(ticket.Embedding)) 
                .Take(2)
                .ToListAsync();

            string context = "";
            
            if (relevantFaqs.Any())
            {
                context += "--- OFFICIAL KNOWLEDGE BASE (STRICT POLICIES) ---\n";
                foreach (var faq in relevantFaqs)
                {
                    context += $"- Rule/FAQ: {faq.Question}\n- Action to Take: {faq.Answer}\n\n";
                }
            }

            if (similarResolvedTickets.Any())
            {
                context += "--- PAST SIMILAR TICKETS ---\n";
                foreach (var t in similarResolvedTickets)
                {
                    context += $"- Past Issue: {t.Title}\n- Solution Used: {t.Description}\n\n";
                }
            }

            if (string.IsNullOrEmpty(context)) 
            {
                context = "No specific rules or past tickets found. Write a polite acknowledgment saying the team is looking into it.";
            }

            string draft = await _aiService.GenerateDraftReplyAsync(ticket.Description ?? ticket.Title ?? "", context);
            
            ticket.RagDraftReply = draft; 
            await _context.SaveChangesAsync();

            return Ok(new { draftReply = draft });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to generate AI reply.", error = ex.Message });
        }
    }
}