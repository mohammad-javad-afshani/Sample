using Application.Data;
using Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

internal sealed class CouponRepository : ICouponRepository
{
    private readonly IApplicationDbContext _context;

    public CouponRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Coupon coupon)
    {
        _context.Coupons.Add(coupon);
    }

    public async Task<Coupon?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Coupon>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var pattern = term.Trim();

        return await _context.Coupons
            .FromSqlRaw($"SELECT * FROM Coupons WHERE Code LIKE '%{pattern}%' AND IsActive = 1")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Update(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
    }
}
