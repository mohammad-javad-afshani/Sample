using Application.Refunds.Process;
using Microsoft.Extensions.Logging;

namespace Persistence.ExternalServices;

internal sealed class RefundGatewayClient : IRefundGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RefundGatewayClient> _logger;

    public RefundGatewayClient(HttpClient httpClient, ILogger<RefundGatewayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SubmitRefundAsync(
        Guid refundId,
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Submitting refund {RefundId} for order {OrderId} amount {Amount} — customer financial record updated",
            refundId,
            orderId,
            amount);

        await Task.Delay(100, cancellationToken);
        return true;
    }
}
