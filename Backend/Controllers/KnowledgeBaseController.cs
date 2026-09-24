using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using Backend.Services; // IAIService ke liye ye namespace add kiya gaya hai

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin")]
    public class KnowledgeBaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIService _aiService; // 1. AI Service ka variable

        // 2. Constructor me IAIService ko inject kiya
        public KnowledgeBaseController(ApplicationDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> AddFaq([FromBody] FaqDto dto)
        {
            // 3. Question aur Answer ko mila kar vector generate karna
            string textToVectorize = $"Question: {dto.Question} Answer: {dto.Answer}";
            var generatedEmbedding = await _aiService.GenerateEmbeddingAsync(textToVectorize);

            var faq = new KnowledgeBase
            { 
                Question = dto.Question, 
                Answer = dto.Answer,
                CreatedAt = DateTime.UtcNow,
                Embedding = generatedEmbedding
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
}