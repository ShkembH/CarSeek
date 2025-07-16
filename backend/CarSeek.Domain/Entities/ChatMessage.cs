using CarSeek.Domain.Common;

namespace CarSeek.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public Guid ListingId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
    }
}
