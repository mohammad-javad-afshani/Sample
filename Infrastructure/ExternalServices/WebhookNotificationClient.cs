using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Application.Notifications;
using Microsoft.Extensions.Logging;

namespace Persistence.ExternalServices;

internal sealed class WebhookNotificationClient : IWebhookNotificationClient
{
    private const string WebhookSigningKey = "whsec_benchmark_sample_do_not_commit_real_keys";

    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookNotificationClient> _logger;

    public WebhookNotificationClient(HttpClient httpClient, ILogger<WebhookNotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task DispatchAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            payload.OrderId,
            payload.PaymentId,
            payload.Amount,
            payload.EventType,
            TimestampUtc = DateTime.UtcNow
        };

        var signature = ComputeSignature(body.ToString() ?? string.Empty);

        _ = Task.Run(async () =>
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "events/payment-completed")
                {
                    Content = JsonContent.Create(body)
                };
                request.Headers.Add("X-Webhook-Signature", signature);

                await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webhook dispatch failed for order {OrderId}", payload.OrderId);
            }
        }, CancellationToken.None);

        return Task.CompletedTask;
    }

    private static string ComputeSignature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(WebhookSigningKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }
}
