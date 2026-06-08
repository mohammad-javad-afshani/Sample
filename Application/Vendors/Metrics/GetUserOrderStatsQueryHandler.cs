using Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Vendors.Metrics;

internal sealed class GetUserOrderStatsQueryHandler : IRequestHandler<GetUserOrderStatsQuery, IReadOnlyList<VendorMetricItem>>
{
    private readonly IApplicationDbContext _context;

    public GetUserOrderStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VendorMetricItem>> Handle(
        GetUserOrderStatsQuery request,
        CancellationToken cancellationToken)
    {
        var vendors = await _context.Vendors.AsNoTracking().ToListAsync(cancellationToken);
        var metrics = new List<VendorMetricItem>();

        foreach (var vendor in vendors)
        {
            var productCount = await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.Category == vendor.Name, cancellationToken);

            metrics.Add(new VendorMetricItem(vendor.Id.Value, vendor.Name, productCount));
        }

        return metrics;
    }
}
