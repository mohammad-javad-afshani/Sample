namespace Domain.Vendors;

public readonly record struct VendorId(Guid Value)
{
    public static VendorId New() => new(Guid.NewGuid());
}
