using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations;

public class CarImageConfiguration : IEntityTypeConfiguration<CarImage>
{
    public void Configure(EntityTypeBuilder<CarImage> builder)
    {
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ci => ci.AltText)
            .HasMaxLength(200);

        builder.Property(ci => ci.DisplayOrder)
            .IsRequired();

        builder.Property(ci => ci.IsPrimary)
            .IsRequired();

        // Configure relationship
        builder.HasOne(ci => ci.CarListing)
            .WithMany(cl => cl.Images)
            .HasForeignKey(ci => ci.CarListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for better query performance
        builder.HasIndex(ci => new { ci.CarListingId, ci.DisplayOrder });
        builder.HasIndex(ci => new { ci.CarListingId, ci.IsPrimary });
    }
}
