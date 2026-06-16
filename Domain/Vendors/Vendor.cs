namespace Domain.Vendors;

public class Vendor
{
    public VendorId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public VendorStatus Status { get; private set; }
    public string InternalNotes { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private Vendor() { }

    public Vendor(string name, string email, string taxId)
    {
        Id = VendorId.New();
        Name = name.Trim();
        Email = email.Trim();
        TaxId = taxId.Trim();
        Status = VendorStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateContact(string name, string email, string taxId)
    {
        Name = name.Trim();
        Email = email.Trim();
        TaxId = taxId.Trim();
    }

    public void Suspend() => Status = VendorStatus.Suspended;

    public void Archive() => Status = VendorStatus.Archived;
}
