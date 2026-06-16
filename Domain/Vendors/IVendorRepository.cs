namespace Domain.Vendors;

public interface IVendorRepository
{
    void Add(Vendor vendor);
    void Update(Vendor vendor);
    Task<Vendor?> FindByIdAsync(VendorId id, CancellationToken cancellationToken = default);
    Task<Vendor?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vendor>> SearchUnsafeAsync(string term, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Vendor> Items, int TotalCount)> ListPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
