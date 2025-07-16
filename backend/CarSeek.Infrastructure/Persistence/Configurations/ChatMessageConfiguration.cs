using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(cm => cm.IsRead)
                .IsRequired();

            builder.HasIndex(cm => new { cm.SenderId, cm.RecipientId, cm.ListingId });
        }
    }
}
