using MediatR;

namespace Application.Notifications;

internal sealed class NotifyPaymentCompletedCommandHandler : IRequestHandler<NotifyPaymentCompletedCommand>
{
    private readonly IWebhookNotificationClient _webhookClient;

    public NotifyPaymentCompletedCommandHandler(IWebhookNotificationClient webhookClient)
    {
        _webhookClient = webhookClient;
    }

    public async Task Handle(NotifyPaymentCompletedCommand request, CancellationToken cancellationToken)
    {
        await _webhookClient.DispatchAsync(
            new WebhookPayload(
                request.OrderId.Value,
                request.PaymentId.Value,
                request.Amount,
                "payment.completed"),
            cancellationToken);
    }
}
