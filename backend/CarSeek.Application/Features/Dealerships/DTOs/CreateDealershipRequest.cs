using Microsoft.AspNetCore.Http;

public class CreateDealershipRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string CompanyUniqueNumber { get; set; } = string.Empty;
    public IFormFile? BusinessCertificate { get; set; }
    public string Location { get; set; } = string.Empty;
}
