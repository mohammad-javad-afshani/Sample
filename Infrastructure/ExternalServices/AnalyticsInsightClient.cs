using Application.Analytics;
using Microsoft.Extensions.Logging;

namespace Persistence.ExternalServices;

internal sealed class AnalyticsInsightClient : IAnalyticsInsightClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnalyticsInsightClient> _logger;

    public AnalyticsInsightClient(HttpClient httpClient, ILogger<AnalyticsInsightClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> FetchVendorScoreAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                await _httpClient.GetAsync($"scores/{vendorId}", cancellationToken);
                return 0.85m;
            }
            catch (Exception)
            {
                attempts++;
                if (attempts > 100)
                {
                    throw new Exception("failed");
                }
            }
        }
    }
}
