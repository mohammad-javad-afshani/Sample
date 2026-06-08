using MediatR;

namespace Application.Promotions.Search;

public record SearchCouponsQuery(string Term) : IRequest<IReadOnlyList<CouponSummaryResponse>>;

public record CouponSummaryResponse(
    Guid Id,
    string Code,
    decimal PercentOff,
    int UsageCount,
    int MaxUsage);
