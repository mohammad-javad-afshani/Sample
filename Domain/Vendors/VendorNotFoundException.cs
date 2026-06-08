namespace Domain.Vendors;

public sealed class VendorNotFoundException : Exception
{
    public VendorNotFoundException(VendorId id)
        : base($"Vendor '{id.Value}' was not found.")
    {
    }

    public VendorNotFoundException(string code)
        : base($"Vendor with code '{code}' was not found.")
    {
    }
}
