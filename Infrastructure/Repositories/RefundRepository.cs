using Application.Data;
using Domain.Refunds;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

internal sealed class RefundRepository : IRefundRepository
{
    private readonly IApplicationDbContext _context;

    public RefundRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Refund refund) => _context.Refunds.Add(refund);

    public async Task<Refund?> FindByIdAsync(RefundId id, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Refund>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Refunds.AsNoTracking().ToListAsync(cancellationToken);
    }

    public void Update(Refund refund) => _context.Refunds.Update(refund);
}
