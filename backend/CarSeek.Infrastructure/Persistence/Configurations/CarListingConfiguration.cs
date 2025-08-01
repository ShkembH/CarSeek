using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations;

public class CarListingConfiguration : IEntityTypeConfiguration<CarListing>
{
    public void Configure(EntityTypeBuilder<CarListing> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(x => x.Make)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");
            
        builder.Property(x => x.Features)
            .HasMaxLength(1000);

        // Add indexes for better search performance
        builder.HasIndex(x => x.Status); // For filtering active listings
        builder.HasIndex(x => x.Make); // For brand filtering
        builder.HasIndex(x => x.Model); // For model filtering
        builder.HasIndex(x => x.Year); // For year range filtering
        builder.HasIndex(x => x.Price); // For price range filtering
        builder.HasIndex(x => x.UserId); // For user's listings
        builder.HasIndex(x => x.DealershipId); // For dealership listings
        
        // Composite indexes for common search combinations
        builder.HasIndex(x => new { x.Status, x.Make, x.Model });
        builder.HasIndex(x => new { x.Status, x.Year, x.Price });
        builder.HasIndex(x => new { x.Status, x.UserId });
        
        // Full-text search index for title and description
        builder.HasIndex(x => new { x.Title, x.Description })
            .HasDatabaseName("IX_CarListing_Title_Description_FullText");
    }
}
