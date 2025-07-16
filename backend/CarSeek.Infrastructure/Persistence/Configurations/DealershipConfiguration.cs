using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations;

public class DealershipConfiguration : IEntityTypeConfiguration<Dealership>
{
    public void Configure(EntityTypeBuilder<Dealership> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(d => d.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.Website)
            .HasMaxLength(255);

        builder.Property(d => d.CompanyUniqueNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.BusinessCertificatePath)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.Location)
            .HasMaxLength(255)
            .IsRequired();

        // Configure owned entity (Value Object)
        builder.OwnsOne(d => d.Address, address =>
        {
            address.Property(a => a.Street)
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasMaxLength(50)
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .IsRequired();

            address.Property(a => a.Country)
                .HasMaxLength(100)
                .IsRequired();
        });

        // Relationships
        builder.HasOne(d => d.User)
            .WithOne(u => u.Dealership)
            .HasForeignKey<Dealership>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Listings)
            .WithOne(c => c.Dealership)
            .HasForeignKey(c => c.DealershipId);
    }
}
