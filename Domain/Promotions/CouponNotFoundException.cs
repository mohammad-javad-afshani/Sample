namespace Domain.Promotions;

public sealed class CouponNotFoundException : Exception
{
    public CouponNotFoundException(string code)
        : base($"Coupon '{code}' was not found or is inactive.")
    {
    }
}
