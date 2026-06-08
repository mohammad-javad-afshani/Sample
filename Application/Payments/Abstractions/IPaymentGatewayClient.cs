namespace Application.Payments.Abstractions;

public sealed record PaymentGatewayRequest(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string? IdempotencyKey);

public sealed record PaymentGatewayResult(
    bool Succeeded,
    string? ExternalReference,
    string? FailureReason);

public interface IPaymentGatewayClient
{
    Task<PaymentGatewayResult> CreatePaymentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default);
}
