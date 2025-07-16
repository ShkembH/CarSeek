using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations;

public class SavedListingConfiguration : IEntityTypeConfiguration<SavedListing>
{
    public void Configure(EntityTypeBuilder<SavedListing> builder)
    {
        builder.HasKey(sl => sl.Id);

        // Relationships
        builder.HasOne(sl => sl.User)
            .WithMany(u => u.SavedListings)
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.NoAction);  // Changed from Cascade to NoAction

        builder.HasOne(sl => sl.CarListing)
            .WithMany(cl => cl.SavedByUsers)
            .HasForeignKey(sl => sl.CarListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create a unique constraint to prevent duplicate saved listings
        builder.HasIndex(sl => new { sl.UserId, sl.CarListingId })
            .IsUnique();
    }
}
