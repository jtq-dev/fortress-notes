using FortressApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FortressApi.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        b.Entity<Note>()
            .HasIndex(x => new { x.UserId, x.UpdatedAtUtc });

        b.Entity<AuditEvent>()
            .HasIndex(x => x.CreatedAtUtc);

        base.OnModelCreating(b);
    }
}
