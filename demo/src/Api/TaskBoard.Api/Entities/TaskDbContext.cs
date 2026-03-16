using Microsoft.EntityFrameworkCore;

namespace TaskBoard.Api.Entities;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   SQL-002 — Calls Database.Migrate() at startup via EnsureCreated/Migrate pattern.
///             This causes race conditions in multi-instance deployments.
///             Migrations should be applied via CI/CD pipeline only.
/// </summary>
public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.HasIndex(e => e.UserId);
        });
    }

    // VIOLATION SQL-002: Database.Migrate() at startup
    // In multi-instance deployments, multiple instances racing to apply
    // migrations causes deadlocks and startup timeouts.
    public static void InitializeDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        db.Database.Migrate();  // VIOLATION: should be in CI/CD pipeline
    }
}
