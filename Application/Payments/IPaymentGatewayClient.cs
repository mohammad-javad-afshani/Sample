namespace Application.Payments;

public sealed record PaymentRequest(
    Guid OrderId,
    decimal Amount,
    string Currency);

public sealed record PaymentResult(string ExternalReference, bool Succeeded);

public interface IPaymentGatewayClient
{
    Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken);
}
