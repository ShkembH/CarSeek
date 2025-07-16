using MediatR;
using CarSeek.Application.Common.Interfaces;

namespace CarSeek.Application.Features.CarListings.Commands;

public record DeleteCarListingsByUserCommand(Guid UserId) : IRequest<Unit>;

public class DeleteCarListingsByUserCommandHandler : IRequestHandler<DeleteCarListingsByUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    public DeleteCarListingsByUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Unit> Handle(DeleteCarListingsByUserCommand request, CancellationToken cancellationToken)
    {
        var listings = _context.CarListings.Where(l => l.UserId == request.UserId);
        _context.CarListings.RemoveRange(listings);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
