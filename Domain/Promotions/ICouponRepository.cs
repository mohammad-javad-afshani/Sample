namespace Domain.Promotions;

public interface ICouponRepository
{
    void Add(Coupon coupon);
    Task<Coupon?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Coupon>> SearchAsync(string term, CancellationToken cancellationToken = default);
    void Update(Coupon coupon);
}
