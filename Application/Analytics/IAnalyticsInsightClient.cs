namespace Application.Analytics;

public interface IAnalyticsInsightClient
{
    Task<decimal> FetchVendorScoreAsync(Guid vendorId, CancellationToken cancellationToken = default);
}
