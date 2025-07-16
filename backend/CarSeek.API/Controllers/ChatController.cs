using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarSeek.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IApplicationDbContext _dbContext;

        public ChatController(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/Chat/history?userId=...&listingId=...
        [HttpGet("history")]
        public async Task<ActionResult<List<ChatMessage>>> GetChatHistory([FromQuery] Guid userId, [FromQuery] Guid listingId)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized();

            var messages = await _dbContext.ChatMessages
                .Where(m =>
                    ((m.SenderId == currentUserId && m.RecipientId == userId) ||
                     (m.SenderId == userId && m.RecipientId == currentUserId)) &&
                    m.ListingId == listingId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        // GET: api/Chat/conversations
        [HttpGet("conversations")]
        public async Task<ActionResult<List<object>>> GetConversations()
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized();

            // Fetch all relevant messages into memory first
            var messages = await _dbContext.ChatMessages
                .Where(m => m.SenderId == currentUserId || m.RecipientId == currentUserId)
                .ToListAsync();

            var conversations = messages
                .GroupBy(m => new { m.ListingId, OtherUserId = m.SenderId == currentUserId ? m.RecipientId : m.SenderId })
                .Select(g => new
                {
                    ListingId = g.Key.ListingId,
                    OtherUserId = g.Key.OtherUserId,
                    LastMessage = g.OrderByDescending(m => m.CreatedAt)
                        .Select(m => new {
                            m.Id,
                            m.Message,
                            m.CreatedAt,
                            m.SenderId,
                            m.RecipientId,
                            m.IsRead
                        })
                        .FirstOrDefault(),
                    UnreadCount = g.Count(m => m.RecipientId == currentUserId && !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessage.CreatedAt)
                .ToList();

            return Ok(conversations);
        }

        // POST: api/Chat/mark-read
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkReadRequest request)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized();

            var messages = await _dbContext.ChatMessages
                .Where(m => m.SenderId == request.UserId && m.RecipientId == currentUserId && m.ListingId == request.ListingId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in messages)
            {
                msg.IsRead = true;
            }
            await _dbContext.SaveChangesAsync(default);
            return Ok(new { updated = messages.Count });
        }

        public class MarkReadRequest
        {
            public Guid UserId { get; set; }
            public Guid ListingId { get; set; }
        }

        // DELETE: api/Chat/conversation?userId=...&listingId=...
        [HttpDelete("conversation")]
        public async Task<IActionResult> DeleteConversation([FromQuery] Guid userId, [FromQuery] Guid listingId)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized();

            var messages = await _dbContext.ChatMessages
                .Where(m =>
                    ((m.SenderId == currentUserId && m.RecipientId == userId) ||
                     (m.SenderId == userId && m.RecipientId == currentUserId)) &&
                    m.ListingId == listingId)
                .ToListAsync();

            _dbContext.ChatMessages.RemoveRange(messages);
            await _dbContext.SaveChangesAsync(default);
            return Ok(new { deleted = messages.Count });
        }

        // GET: api/Chat/user-info?userId=...
        [HttpGet("user-info")]
        public async Task<ActionResult<object>> GetUserInfo([FromQuery] Guid userId)
        {
            Console.WriteLine($"Looking up userId: {userId}");
            var user = await _dbContext.Users
                .Include(u => u.Dealership)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId.ToString());
            if (user == null)
            {
                Console.WriteLine("User not found!");
                return NotFound();
            }
            Console.WriteLine($"Found user: {user.FirstName} {user.LastName}");
            return Ok(new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role.ToString(),
                companyName = user.Dealership != null ? user.Dealership.Name : null
            });
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
                return userId;
            return null;
        }
    }
}
