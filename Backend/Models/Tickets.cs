using System;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace Backend.Models;

public class Ticket
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = string.Empty;
    
    // File Storage Fields
    public string? AudioUrl { get; set; } 
    public string? ImageUrl { get; set; }

    // --- DAY 4: NEW AI FIELDS ---
    public string? AiTranscription { get; set; } // Audio to text
    public string? AiImageAnalysis { get; set; } // Vision analysis
    public string? AiSentiment { get; set; } // Happy, Angry, Neutral
    public string? AiSummary { get; set; } // Short summary

    public string? AiCategory { get; set; }
    public string? SuggestedResponse { get; set; } // RAG generated response
 
    // Embeddings (Vector Search ke liye - 384 dimensions for HuggingFace MiniLM)
    [Column(TypeName = "vector(384)")]
    public Vector? Embedding { get; set; }
    
    // Relationships & Timestamps
    public int CustomerId { get; set; }
    public User? Customer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}