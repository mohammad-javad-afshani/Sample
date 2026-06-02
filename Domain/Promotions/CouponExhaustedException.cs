namespace Domain.Promotions;

public sealed class CouponExhaustedException : Exception
{
    public CouponExhaustedException(string code)
        : base($"Coupon '{code}' has reached its usage limit.")
    {
    }
}
