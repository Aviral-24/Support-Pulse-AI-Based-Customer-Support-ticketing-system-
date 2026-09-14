using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Pgvector.EntityFrameworkCore;

namespace Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketNote> TicketNotes => Set<TicketNote>();
    public DbSet<KnowledgeBase> KnowledgeBases => Set<KnowledgeBase>();
    public DbSet<AuditLog> AuditLogs { get; set; }

    // Is method ko class ke andar rakhna zaroori hai
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ye line PostgreSQL me vector extension load karegi
        modelBuilder.HasPostgresExtension("vector");
    }
}