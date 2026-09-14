// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Backend.Data;
// using Backend.Models;
// using Backend.DTOs;
// using Backend.Services.Async; 
// using Backend.Services;
// using System.Security.Claims;
// using Backend.Services.Pdf;
// using Pgvector.EntityFrameworkCore; // Day 4: pgvector methods ke liye zaroori
// using Backend.Services;

// namespace Backend.Controllers;

// [ApiController]
// [Route("api/v1/[controller]")]
// [Authorize] // Requires Login globally for this controller
// public class TicketsController : ControllerBase
// {
//     private readonly ApplicationDbContext _context;
//     private readonly IStorageService _storageService;
//     private readonly ITicketQueue _ticketQueue; 
    
//     public TicketsController(ApplicationDbContext context, IStorageService storageService, ITicketQueue ticketQueue) 
//     {
//         _context = context;
//         _storageService = storageService;
//         _ticketQueue = ticketQueue; 
//     }

//     // 1. PUBLIC TICKET SUBMISSION (Customers Only) - DAY 4 INTEGRATED 🚀
//     [HttpPost]
//     [Authorize(Roles = "Customer")] // STRICT: Sirf Customer hi ticket bana sakta hai
//     public async Task<IActionResult> CreateTicket([FromForm] CreateTicketDto request)
//     {
//         var customerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
//         string? audioUrl = null;
//         string? imageUrl = null;

//         try
//         {
//             // S3 Uploads (Ye fast hain, isme time nahi lagta)
//             if (request.AudioFile != null)
//                 audioUrl = await _storageService.UploadFileAsync(request.AudioFile, "uploads/audio");

//             if (request.ImageFile != null)
//                 imageUrl = await _storageService.UploadFileAsync(request.ImageFile, "uploads/images");
//         }
//         catch (Exception ex)
//         {
//             return BadRequest(new { message = $"File upload failed: {ex.Message}" });
//         }

//         var ticket = new Ticket
//         {
//             Title = request.Title,
//             Description = request.Description,
//             Category = request.Category,
//             CustomerId = customerId,
//             AudioUrl = audioUrl,
//             ImageUrl = imageUrl,
//             Status = "Open",
//             CreatedAt = DateTime.UtcNow
//         };

//         // 1. Fast Save: Ticket ko turant database me save kiya
//         _context.Tickets.Add(ticket);
//         await _context.SaveChangesAsync();

//         // 2. AI Trigger: Ticket ID ko Background Queue me daal diya taaki AI apna kaam aaram se kare
//         await _ticketQueue.EnqueueTicketAsync(ticket.Id);
//         Console.WriteLine($"\n[DEBUG - CONTROLLER] Ticket {ticket.Id} sent to AI Background Queue!\n");

// // 1. Ticket ko normal database me save karein
//     _context.Tickets.Add(ticket);
//     await _context.SaveChangesAsync();

//     // 2. 🔥 MAIN MAGIC: Ticket ID ko AI Worker (Redis Queue) ke paas bhej dein
//     await ticketQueue.QueueTicketAsync(ticket.Id);
//         // 3. Instant Response: User ko turant success mil gaya, bina AI ka wait kiye!
//         return Ok(new { message = "Ticket created successfully! AI is analyzing it in the background.", ticketId = ticket.Id });
//     }

//     // GET: Fetch a single ticket with Expiring S3 URLs
//     [HttpGet("{id}")]
//     [Authorize] // Customer aur Agent dono dekh sakte hain
//     public async Task<IActionResult> GetTicket(int id)
//     {
//         var ticket = await _context.Tickets.FindAsync(id);
//         if (ticket == null) return NotFound();

//         // Database me save ki hui S3 Object Key ko Signed URL me convert karna
//         var response = new 
//         {
//             ticket.Id,
//             ticket.Title,
//             ticket.Description,
//             ImageUrl = _storageService.GenerateSignedUrl(ticket.ImageUrl), 
//             AudioUrl = _storageService.GenerateSignedUrl(ticket.AudioUrl)
//         };

//         return Ok(response);
//     }

//     // 2. TICKET LIST DASHBOARD WITH PAGINATION & FILTERS (Agents/Admins)
//     [HttpGet]
//     [Authorize(Roles = "Agent,Admin")] // STRICT: Sirf Agent ya Admin hi tickets dekh sakte hain
//     public async Task<IActionResult> GetTickets([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
//     {
//         var query = _context.Tickets.Include(t => t.Customer).AsQueryable();

//         if (!string.IsNullOrEmpty(status))
//             query = query.Where(t => t.Status.ToLower() == status.ToLower());

//         if (!string.IsNullOrEmpty(search))
//             query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

//         var totalTickets = await query.CountAsync();
//         var tickets = await query
//             .OrderByDescending(t => t.CreatedAt)
//             .Skip((page - 1) * pageSize)
//             .Take(pageSize)
//             .Select(t => new {
//              t.Id, 
//              t.Title,
//              t.Description, 
//              t.AudioUrl,       
//              t.ImageUrl,
//              t.Status, 
//              t.Category,
//              CustomerName = t.Customer!.Name,
//              AiSummary = t.AiSummary,     
//              AiSentiment = t.AiSentiment,   
//              t.CreatedAt })
//             .ToListAsync();

//         return Ok(new { totalTickets, page, pageSize, tickets });
//     }

//     // 3. STATUS WORKFLOW UPDATE (Agents/Admins)
//     [HttpPut("{id}/status")]
//     [Authorize(Roles = "Agent,Admin")] // STRICT: Status update sirf Agent karega
//     public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusDto request, [FromServices] IAuditService _auditService)
//     {
//         var ticket = await _context.Tickets.FindAsync(id);
//         if (ticket == null) return NotFound("Ticket not found.");

//         var validStatuses = new[] { "Open", "In Progress", "Resolved" };
//         if (!validStatuses.Contains(request.Status)) return BadRequest("Invalid status.");
         
//         ticket.Status = request.Status;
//         await _context.SaveChangesAsync();

//         // Audit Trail Logging
//         var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
//         await _auditService.LogActionAsync(agentId, $"Changed status to {request.Status}", "Ticket", id, "Status updated via Agent Dashboard");

//         return Ok(new { message = $"Ticket status updated to {request.Status}." });
//     }
    
//     // 4. ADD INTERNAL NOTES (Agents Only)
//     [HttpPost("{id}/notes")]
//     [Authorize(Roles = "Agent,Admin")] // STRICT
//     public async Task<IActionResult> AddNote(int id, [FromBody] AddNoteDto request)
//     {
//         var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
//         var ticketExists = await _context.Tickets.AnyAsync(t => t.Id == id);
//         if (!ticketExists) return NotFound("Ticket not found.");

//         var note = new TicketNote
//         {
//             TicketId = id,
//             AgentId = agentId,
//             Note = request.Note
//         };

//         _context.TicketNotes.Add(note);
//         await _context.SaveChangesAsync();
//         return Ok(new { message = "Note added successfully!" });
//     }
   
//     // 5. DOWNLOAD TICKET PDF SUMMARY (Agents/Admins)
//     [HttpGet("{id}/pdf")]
//     [Authorize(Roles = "Agent,Admin")] 
//     public async Task<IActionResult> DownloadTicketPdf(int id, [FromServices] ITicketPdfGenerator _pdfGenerator, [FromServices] IAuditService _auditService)
//     {
//         var ticket = await _context.Tickets
//             .Include(t => t.Customer)
//             .FirstOrDefaultAsync(t => t.Id == id);

//         if (ticket == null) return NotFound("Ticket not found.");

//         try 
//         {
//             var pdfBytes = _pdfGenerator.GenerateTicketSummary(ticket);
            
//             // Audit Trail Logging 
//             var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
//             await _auditService.LogActionAsync(agentId, "Downloaded PDF Summary", "Ticket", id, "PDF generated containing AI Insights");

//             return File(pdfBytes, "application/pdf", $"Ticket_{ticket.Id}_Summary.pdf");
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, $"PDF Generation failed: {ex.Message}");
//         }
//     }

//     // 6. SEMANTIC VECTOR SEARCH / RAG (Day 4 Integration) 🔥
//     [HttpGet("semantic-search")]
//     [Authorize(Roles = "Agent,Admin")] // Sirf agent/admin search kar sakte hain
//     public async Task<IActionResult> SemanticSearch([FromQuery] string query, [FromServices] IAIService _aiService)
//     {
//         if (string.IsNullOrWhiteSpace(query))
//             return BadRequest("Search query cannot be empty.");

//         try
//         {
//             // 1. Text Query ko Vector Embedding me convert karna (Groq/HuggingFace API ke through)
//             var queryVector = await _aiService.GenerateEmbeddingAsync(query);

//             // 2. pgvector ka use karke Database me Cosine Similarity Search karna
//             // EF Core me .CosineDistance() extension method pgvector ke through direct SQL level par search karta hai
//             var similarTickets = await _context.Tickets
//                 .Where(t => t.Embedding != null) // Sirf unme jinka AI enrichment complete ho chuka hai
//                 .OrderBy(t => t.Embedding!.CosineDistance(queryVector))
//                 .Take(5) // Top 5 closest matches
//                 .Select(t => new {
//                     t.Id,
//                     t.Title,
//                     t.AiCategory,
//                     t.AiSummary,
//                     t.Status,
//                     // Distance jitni kam hogi (0 ke kareeb), result utna accurate hoga
//                     Distance = t.Embedding!.CosineDistance(queryVector) 
//                 })
//                 .ToListAsync();

//             return Ok(similarTickets);
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, $"Semantic search failed: {ex.Message}");
//         }
//     }
// }

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
    [Authorize(Roles = "Agent,Admin")]
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
    [Authorize(Roles = "Agent,Admin")]
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

    // 🔥 THE SILVER BULLET: Direct AI Test Endpoint
    [HttpGet("{id}/force-ai-test")]
    [Authorize] 
    public async Task<IActionResult> ForceAiTest(int id, [FromServices] IAIService _aiService)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound("Ticket not found.");

        try
        {
            // 1. Text Analytics (Groq)
            var aiResult = await _aiService.AnalyzeTicketAsync(ticket.Title ?? "", ticket.Description ?? "");
            ticket.AiSummary = aiResult.Summary;
            ticket.AiSentiment = aiResult.Sentiment;

            // 2. Vector Embeddings (HuggingFace)
            string textToEmbed = $"Title: {ticket.Title}. Details: {ticket.Description}. Sentiment: {ticket.AiSentiment}";
            ticket.Embedding = await _aiService.GenerateEmbeddingAsync(textToEmbed);

            ticket.Status = "In Progress";
            await _context.SaveChangesAsync(); // Database me vectors save!

            return Ok(new { 
                message = "🔥 AI Enrichment SUCCESSFUL! Vectors are saved to Database.", 
                summary = ticket.AiSummary, 
                sentiment = ticket.AiSentiment 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "AI API Failed", error = ex.Message });
        }
    }
}