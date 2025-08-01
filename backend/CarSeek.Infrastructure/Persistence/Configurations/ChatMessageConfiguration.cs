using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CarSeek.Domain.Entities;

namespace CarSeek.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(x => x.SenderId)
            .IsRequired();
            
        builder.Property(x => x.RecipientId)
            .IsRequired();
            
        builder.Property(x => x.ListingId)
            .IsRequired();
            
        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        // Add indexes for better chat performance
        builder.HasIndex(x => x.SenderId); // For finding messages sent by user
        builder.HasIndex(x => x.RecipientId); // For finding messages received by user
        builder.HasIndex(x => x.ListingId); // For finding messages about specific listing
        builder.HasIndex(x => x.CreatedAt); // For chronological ordering
        builder.HasIndex(x => x.IsRead); // For unread message queries
        
        // Composite indexes for common chat queries
        builder.HasIndex(x => new { x.SenderId, x.RecipientId, x.ListingId });
        builder.HasIndex(x => new { x.RecipientId, x.IsRead }); // For unread messages per user
        builder.HasIndex(x => new { x.ListingId, x.CreatedAt }); // For listing conversation history
        builder.HasIndex(x => new { x.SenderId, x.CreatedAt }); // For user's sent messages
        builder.HasIndex(x => new { x.RecipientId, x.CreatedAt }); // For user's received messages
    }
}
