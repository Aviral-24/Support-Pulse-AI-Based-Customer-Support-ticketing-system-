using System;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector; // 'PgVector' ya 'Pgvector' jo aapke package me ho

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
    public string? AiTranscription { get; set; } 
    public string? AiImageAnalysis { get; set; } 
    public string? AiSentiment { get; set; } 
    public string? AiSummary { get; set; } 

    public string? AiCategory { get; set; }
    public string? SuggestedResponse { get; set; } 
 
    // aur TypeName me 'vector(384)' laga diya Postgres migrations ke liye.
    [Column(TypeName = "vector(384)")]
    public Vector? Embedding { get; set; }

    // Relationships & Timestamps
    public int CustomerId { get; set; }
    public User? Customer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}