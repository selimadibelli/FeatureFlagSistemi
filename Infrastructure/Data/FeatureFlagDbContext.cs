using Microsoft.EntityFrameworkCore;
using FeatureFlagSystem.Domain.Entities;

namespace FeatureFlagSystem.Infrastructure.Data;

public class FeatureFlagDbContext : DbContext
{
    public FeatureFlagDbContext(DbContextOptions<FeatureFlagDbContext> options) : base(options)
    {
    }

    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<PilotWhitelist> PilotWhitelists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // FeatureFlag konfigürasyonu
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            
            // Unique constraint
            entity.HasIndex(e => e.Name).IsUnique();
            
            // Timestamps
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        // PilotWhitelist konfigürasyonu
        modelBuilder.Entity<PilotWhitelist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserIdentifier).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserType).HasMaxLength(50);
            entity.Property(e => e.MinVersion).HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            
            // Timestamps
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            
            // Foreign key
            entity.HasOne(e => e.FeatureFlag)
                  .WithMany(f => f.PilotWhitelists)
                  .HasForeignKey(e => e.FeatureFlagId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Composite index for performance
            entity.HasIndex(e => new { e.FeatureFlagId, e.UserIdentifier }).IsUnique();
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Örnek feature flag'ler
        modelBuilder.Entity<FeatureFlag>().HasData(
            new FeatureFlag
            {
                Id = 1,
                Name = "OpenBankingPilot",
                Description = "Open Banking pilot özelliği",
                IsEnabled = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new FeatureFlag
            {
                Id = 2,
                Name = "SealSigningPilot",
                Description = "Seal Signing pilot özelliği (kullanımdan kaldırıldı)",
                IsEnabled = false,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new FeatureFlag
            {
                Id = 3,
                Name = "SealPilot",
                Description = "Seal pilot özelliği",
                IsEnabled = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            },
            new FeatureFlag
            {
                Id = 4,
                Name = "SoftLoginPilot",
                Description = "Soft Login pilot özelliği",
                IsEnabled = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            }
        );

        // Örnek pilot whitelist'ler
        modelBuilder.Entity<PilotWhitelist>().HasData(
            new PilotWhitelist
            {
                Id = 1,
                FeatureFlagId = 1,
                UserIdentifier = "CUST001",
                UserType = "Customer",
                MinVersion = "1.2.0",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            },
            new PilotWhitelist
            {
                Id = 2,
                FeatureFlagId = 3,
                UserIdentifier = "CUST002",
                UserType = "Customer",
                MinVersion = "1.1.0",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(60)
            },
            new PilotWhitelist
            {
                Id = 3,
                FeatureFlagId = 4,
                UserIdentifier = "EMP001",
                UserType = "Employee",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(90)
            }
        );
    }
}
