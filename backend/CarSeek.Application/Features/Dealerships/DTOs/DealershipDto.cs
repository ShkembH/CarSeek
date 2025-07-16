using CarSeek.Domain.ValueObjects;

namespace CarSeek.Application.Features.Dealerships.DTOs;

public class DealershipDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string CompanyUniqueNumber { get; set; } = string.Empty;
    public string BusinessCertificatePath { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
