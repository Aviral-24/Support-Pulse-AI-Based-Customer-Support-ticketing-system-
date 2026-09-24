using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector; // Ye namespace bahut zaroori hai vector ke liye

namespace Backend.Models
{
    public class KnowledgeBase
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Question { get; set; } = string.Empty;
        
        [Required]
        public string Answer { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "vector(384)")]
        public Vector? Embedding { get; set; } 
    }
}