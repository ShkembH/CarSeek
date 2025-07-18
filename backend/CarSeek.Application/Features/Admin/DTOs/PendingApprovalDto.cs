namespace CarSeek.Application.Features.Admin.DTOs;

public class PendingApprovalDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Submitter { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string DealershipName { get; set; } = string.Empty;
    public string DealershipDescription { get; set; } = string.Empty;
    public string AddressStreet { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
    public string AddressPostalCode { get; set; } = string.Empty;
    public string AddressCountry { get; set; } = string.Empty;
    public string DealershipPhoneNumber { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string CompanyUniqueNumber { get; set; } = string.Empty;
    public string BusinessCertificatePath { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string UserPhoneNumber { get; set; } = string.Empty;
    public string UserCountry { get; set; } = string.Empty;
    public string UserCity { get; set; } = string.Empty;
}
