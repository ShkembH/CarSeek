using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Domain.Entities;
using System;
using System.Security.Claims;

namespace CarSeek.API.src
{
    public class ChatHub : Hub
    {
        private readonly IApplicationDbContext _dbContext;

        public ChatHub(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Only authenticated users can send messages
        public async Task SendMessage(string toUserId, string message, string listingId)
        {
            if (Context.UserIdentifier == null)
                throw new HubException("Unauthorized");

            if (!Guid.TryParse(Context.UserIdentifier, out var senderId))
                throw new HubException("Invalid sender ID");
            if (!Guid.TryParse(toUserId, out var recipientId))
                throw new HubException("Invalid recipient ID");
            if (!Guid.TryParse(listingId, out var listingGuid))
                throw new HubException("Invalid listing ID");

            // Persist the message
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                RecipientId = recipientId,
                ListingId = listingGuid,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync(default);

            // Send the message to the recipient
            await Clients.User(toUserId).SendAsync("ReceiveMessage", Context.UserIdentifier, message, listingId);

            // Optionally, send a notification to the recipient
            await Clients.User(toUserId).SendAsync("ReceiveNotification", $"New message on your listing {listingId}");
        }
    }
}
