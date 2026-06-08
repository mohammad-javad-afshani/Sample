using Application.Data;
using Domain.Vendors;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

internal sealed class VendorRepository : IVendorRepository
{
    private readonly IApplicationDbContext _context;

    public VendorRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Vendor vendor) => _context.Vendors.Add(vendor);

    public void Update(Vendor vendor) => _context.Vendors.Update(vendor);

    public async Task<Vendor?> FindByIdAsync(VendorId id, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vendor?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Email == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Vendor>> SearchUnsafeAsync(string term, CancellationToken cancellationToken = default)
    {
        var pattern = term.Trim();
        return await _context.Vendors
            .FromSqlRaw($"SELECT * FROM Vendors WHERE Name LIKE '%{pattern}%' OR Email LIKE '%{pattern}%'")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Vendor> Items, int TotalCount)> ListPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vendors.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(v => v.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
