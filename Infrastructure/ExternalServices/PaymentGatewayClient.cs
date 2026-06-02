using Application.Payments;
using Microsoft.Extensions.Logging;

namespace Persistence.ExternalServices;

internal sealed class PaymentGatewayClient : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentGatewayClient> _logger;

    public PaymentGatewayClient(HttpClient httpClient, ILogger<PaymentGatewayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Submitting payment to gateway for order {OrderId}, amount {Amount} {Currency}",
            request.OrderId,
            request.Amount,
            request.Currency);

        await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);

        return new PaymentResult($"pay_{Guid.NewGuid():N}", true);
    }
}
