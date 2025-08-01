using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);
            
        builder.Property(x => x.Country)
            .HasMaxLength(100);
            
        builder.Property(x => x.City)
            .HasMaxLength(100);

        // Add indexes for better query performance
        builder.HasIndex(x => x.Email).IsUnique(); // Email should be unique
        builder.HasIndex(x => x.Role); // For filtering by user role
        builder.HasIndex(x => x.Status); // For filtering by user status
        builder.HasIndex(x => x.IsActive); // For active user queries
        
        // Composite indexes for common queries
        builder.HasIndex(x => new { x.Role, x.IsActive });
        builder.HasIndex(x => new { x.Status, x.IsActive });
        
        // Full-text search for name searches
        builder.HasIndex(x => new { x.FirstName, x.LastName })
            .HasDatabaseName("IX_User_Name_FullText");
    }
}
