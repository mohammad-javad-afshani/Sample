using Domain.Promotions;
using MediatR;

namespace Application.Promotions.Search;

internal sealed class SearchCouponsQueryHandler : IRequestHandler<SearchCouponsQuery, IReadOnlyList<CouponSummaryResponse>>
{
    private readonly ICouponRepository _couponRepository;

    public SearchCouponsQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<IReadOnlyList<CouponSummaryResponse>> Handle(
        SearchCouponsQuery request,
        CancellationToken cancellationToken)
    {
        var coupons = await _couponRepository.SearchAsync(request.Term, cancellationToken);

        return coupons
            .Select(c => new CouponSummaryResponse(
                c.Id.Value,
                c.Code,
                c.PercentOff,
                c.UsageCount,
                c.MaxUsage))
            .ToList();
    }
}
