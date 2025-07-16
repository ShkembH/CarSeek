using CarSeek.Domain.Entities;

namespace CarSeek.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
