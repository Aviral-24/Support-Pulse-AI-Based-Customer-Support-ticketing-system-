using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Pgvector.EntityFrameworkCore;
using Pgvector;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {  
        // Yeh line race condition block karegi
      modelBuilder.Entity<Ticket>().Property<uint>("xmin").IsRowVersion();
        base.OnModelCreating(modelBuilder);
        
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            modelBuilder.Ignore<Vector>();
        }
        else
        {
            // Normal Production/Development ke liye Postgres extension
            modelBuilder.HasPostgresExtension("vector");
        }
    }
}