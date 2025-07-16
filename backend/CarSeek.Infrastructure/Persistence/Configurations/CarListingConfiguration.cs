using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations;

public class CarListingConfiguration : IEntityTypeConfiguration<CarListing>
{
    public void Configure(EntityTypeBuilder<CarListing> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.Year)
            .IsRequired();

        builder.Property(c => c.Make)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Model)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Mileage)
            .IsRequired();

        builder.Property(c => c.Status)
            .IsRequired();

        // Relationships
        builder.HasOne(c => c.Dealership)
            .WithMany(d => d.Listings)
            .HasForeignKey(c => c.DealershipId)
            .OnDelete(DeleteBehavior.SetNull)  // Changed to SetNull since it's optional
            .IsRequired(false);  // Make the relationship optional

        // Add relationship to User
        builder.HasOne(c => c.User)
            .WithMany()  // User doesn't have a navigation property back to listings
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);  // Changed from Cascade to NoAction to avoid cascade conflicts

        builder.HasMany(c => c.SavedByUsers)
            .WithOne(sl => sl.CarListing)
            .HasForeignKey(sl => sl.CarListingId);
    }
}
