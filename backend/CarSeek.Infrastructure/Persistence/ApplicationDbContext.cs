using Microsoft.EntityFrameworkCore;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Domain.Entities;
using CarSeek.Domain.Enums;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using CarSeek.Infrastructure.Persistence.Configurations;

namespace CarSeek.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<CarListing> CarListings => Set<CarListing>();
    public DbSet<Dealership> Dealerships => Set<Dealership>();
    public DbSet<SavedListing> SavedListings => Set<SavedListing>();
    public DbSet<CarImage> CarImages => Set<CarImage>(); // Move this here as a class property
    // Add this line with the other DbSet properties
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Explicitly map entities to new table names after renaming
        modelBuilder.Entity<User>().ToTable("CarSeekUsers");
        modelBuilder.Entity<CarListing>().ToTable("CarSeekCarListings");
        modelBuilder.Entity<Dealership>().ToTable("CarSeekDealerships");
        modelBuilder.Entity<SavedListing>().ToTable("CarSeekSavedListings");
        modelBuilder.Entity<ActivityLog>().ToTable("CarSeekActivityLogs");
        modelBuilder.Entity<CarImage>().ToTable("CarSeekCarImages");
        modelBuilder.Entity<ChatMessage>().ToTable("ChatMessages"); // Only if ChatMessages table was not renamed

        // Apply CarImage and ChatMessage configuration
        modelBuilder.ApplyConfiguration(new CarImageConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());

        // Seed admin user
        SeedAdminUser(modelBuilder);
    }

    private void SeedAdminUser(ModelBuilder modelBuilder)
    {
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Use static values instead of dynamic ones
        var staticDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var staticSalt = "dGVzdHNhbHQxMjM0NTY3OA=="; // Base64 encoded "testsalt12345678"
        var staticPassword = "Admin123!";

        // Create deterministic hash
        var saltBytes = Convert.FromBase64String(staticSalt);
        var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: staticPassword,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        var finalPasswordHash = $"{staticSalt}.{hashedPassword}";

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminId,
                Email = "admin@CarSeek.com",
                PasswordHash = finalPasswordHash,
                FirstName = "System",
                LastName = "Administrator",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = staticDateTime
            }
        );
    }
}
