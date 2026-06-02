namespace Application.Notifications;

public sealed record WebhookPayload(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string EventType);

public interface IWebhookNotificationClient
{
    Task DispatchAsync(WebhookPayload payload, CancellationToken cancellationToken = default);
}
