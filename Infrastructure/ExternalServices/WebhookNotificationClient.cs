using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Persistence.ExternalServices;

internal sealed class WebhookNotificationClient : IWebhookNotificationClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookNotificationClient> _logger;

    public WebhookNotificationClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WebhookNotificationClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task DispatchAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            payload.OrderId,
            payload.PaymentId,
            payload.Amount,
            payload.EventType,
            TimestampUtc = DateTime.UtcNow
        };

        var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
        var signature = ComputeSignature(jsonBody);

        using var request = new HttpRequestMessage(HttpMethod.Post, "events/payment-completed")
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        request.Headers.Add("X-Webhook-Signature", signature);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Webhook dispatch failed for order {OrderId} with status {StatusCode}",
                payload.OrderId,
                (int)response.StatusCode);
            response.EnsureSuccessStatusCode();
        }
    }

    private string ComputeSignature(string payload)
    {
        var signingKey = _configuration["Webhooks:SigningKey"]
            ?? throw new InvalidOperationException("Webhooks:SigningKey is not configured.");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }
}
