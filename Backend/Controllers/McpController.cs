// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Backend.Data;
// using Backend.Services;
// using System.Security.Claims;

// namespace Backend.Controllers;

// [ApiController]
// [Route("api/v1/[controller]")]
// [Authorize(Roles = "Agent,Admin")] // Sirf authorized agents AI analytics access kar sakte hain
// public class McpController : ControllerBase
// {
//     private readonly ApplicationDbContext _context;
//     private readonly IAuditService _auditService;

//     public McpController(ApplicationDbContext context, IAuditService auditService)
//     {
//         _context = context;
//         _auditService = auditService;
//     }

//     // MCP Tool: AI agent is endpoint ko call karega data analytics ke liye
//     [HttpPost("tools/execute")]
//     public async Task<IActionResult> ExecuteMcpTool([FromBody] McpToolRequest request)
//     {
//         var agentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

//         try
//         {
//             if (request.ToolName == "get_ticket_analytics")
//             {
//                 // Real-time analytics aggregation
//                 var totalTickets = await _context.Tickets.CountAsync();
//                 var openTickets = await _context.Tickets.CountAsync(t => t.Status == "Open");
//                 var resolvedTickets = await _context.Tickets.CountAsync(t => t.Status == "Resolved");
                
//                 // AI Sentiment Breakdown
//                 var angryCustomers = await _context.Tickets.CountAsync(t => t.AiSentiment == "Angry" || t.AiSentiment == "Frustrated");

//                 var analyticsResult = new
//                 {
//                     Total = totalTickets,
//                     Open = openTickets,
//                     Resolved = resolvedTickets,
//                     HighRiskCustomers = angryCustomers,
//                     Summary = $"Currently there are {openTickets} open tickets. {angryCustomers} customers are flagged as high-risk/angry."
//                 };

//                 // Audit Logging (Error fix: null ki jagah 0 use kiya)
//                 await _auditService.LogActionAsync(agentId, "Executed MCP Tool: get_ticket_analytics", "McpServer", 0, "AI retrieved conversational analytics.");

//                 return Ok(new { success = true, data = analyticsResult });
//             }

//             return BadRequest(new { success = false, message = "Tool not recognized." });
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, new { success = false, message = ex.Message });
//         }
//     }
// }

// // MCP Request DTO
// public class McpToolRequest
// {
//     public string ToolName { get; set; } = string.Empty;
//     public Dictionary<string, object>? Parameters { get; set; }
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Mcp;
using System.Text.Json;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Agent")] // Sirf authorized agents hi AI ke through server se baat kar sakte hain
public class McpController : ControllerBase
{
    private readonly SupportPulseMcpServer _mcpServer;

    public McpController(SupportPulseMcpServer mcpServer)
    {
        _mcpServer = mcpServer;
    }

    // AI Tool Request ka JSON Structure
    public class McpRequest
    {
        public string ToolName { get; set; } = string.Empty;
        public JsonElement Parameters { get; set; }
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteTool([FromBody] McpRequest request)
    {
        try
        {
            // 1. Tool 1: get_ticket_analytics
            if (request.ToolName == "get_ticket_analytics")
            {
                // AI se aayi hui parameters ko parse karna
                string? category = request.Parameters.TryGetProperty("category", out var c) ? c.GetString() : null;
                string? status = request.Parameters.TryGetProperty("status", out var s) ? s.GetString() : null;
                string? sentiment = request.Parameters.TryGetProperty("sentiment", out var se) ? se.GetString() : null;

                // Apne internal MCP Server ko call karna
                var resultJson = await _mcpServer.GetTicketAnalyticsAsync(category, status, sentiment);
                
                return Ok(new { success = true, result = JsonSerializer.Deserialize<object>(resultJson) });
            }
            
            // 2. Tool 2: generate_pdf_summary
            else if (request.ToolName == "generate_pdf_summary")
            {
                if (!request.Parameters.TryGetProperty("ticketId", out var tId) || !tId.TryGetInt32(out int ticketId))
                {
                    return BadRequest(new { success = false, message = "Missing or invalid 'ticketId' parameter." });
                }

                // AI ko PDF download karne ke bajaye, hum use Base64 string return kar sakte hain
                // Taki AI document ko read kar sake
                var pdfBytes = await _mcpServer.GeneratePdfSummaryToolAsync(ticketId);
                var base64Pdf = Convert.ToBase64String(pdfBytes);

                return Ok(new { 
                    success = true, 
                    message = $"PDF generated for Ticket #{ticketId}",
                    base64Data = base64Pdf // AI can decode this to read the PDF
                });
            }

            // 3. Unknown Tool
            else
            {
                return BadRequest(new { success = false, message = $"Tool '{request.ToolName}' not found." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}