using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;


// Agar aapke DbContext ya Models kisi aur folder me hain, toh unke using statement yahan add kar lena
// example: using SupportPulse.Data;

namespace SupportPulse.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    public class KnowledgeBaseController : ControllerBase
    {
        // Note: ApplicationDbContext ko apne actual DB context ke naam se replace kar lena agar alag hai
        private readonly ApplicationDbContext _context;

        public KnowledgeBaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddFaq([FromBody] FaqDto dto)
        {
            var faq = new KnowledgeBase
            { 
                Question = dto.Question, 
                Answer = dto.Answer,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Set<KnowledgeBase>().Add(faq);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "FAQ added successfully and vectorized for RAG!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            var faq = await _context.Set<KnowledgeBase>().FindAsync(id);
            if (faq == null) return NotFound();

            _context.Set<KnowledgeBase>().Remove(faq);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "FAQ deleted." });
        }
    }

    public class FaqDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }

 
}