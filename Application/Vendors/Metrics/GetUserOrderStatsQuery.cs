using MediatR;

namespace Application.Vendors.Metrics;

public record GetUserOrderStatsQuery : IRequest<IReadOnlyList<VendorMetricItem>>;

public record VendorMetricItem(Guid VendorId, string Name, int ProductCount);
