using Microsoft.EntityFrameworkCore;
using CarSeek.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace CarSeek.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<CarListing> CarListings { get; }
    DbSet<Dealership> Dealerships { get; }
    DbSet<SavedListing> SavedListings { get; }
    // Add this line to the interface
    DbSet<CarImage> CarImages { get; }
    public DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
