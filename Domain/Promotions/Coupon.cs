namespace Domain.Promotions;

public class Coupon
{
    public CouponId Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public decimal PercentOff { get; private set; }
    public int MaxUsage { get; private set; }
    public int UsageCount { get; internal set; }
    public bool IsActive { get; private set; }

    private Coupon() { }

    public Coupon(string code, decimal percentOff, int maxUsage)
    {
        Id = new CouponId(Guid.NewGuid());
        Code = code;
        PercentOff = percentOff;
        MaxUsage = maxUsage;
        UsageCount = 0;
        IsActive = true;
    }

    public bool HasRemainingUses() => UsageCount < MaxUsage;

    public void RecordRedemption()
    {
        UsageCount++;
    }

    public decimal CalculateDiscount(decimal orderTotal)
    {
        return Math.Round(orderTotal * (PercentOff / 100m), 2, MidpointRounding.AwayFromZero);
    }
}
