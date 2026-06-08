using System.Security.Cryptography;
using System.Text;

namespace WebApplication1;

internal static class ApiKeyAuth
{
    public static bool IsAuthorized(HttpRequest request, IConfiguration configuration, string configPath = "ProductApi:AdminApiKey")
    {
        var configuredKey = configuration[configPath];
        if (string.IsNullOrEmpty(configuredKey))
        {
            return false;
        }

        if (!request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            return false;
        }

        var providedKey = apiKeyHeader.ToString();
        if (providedKey.Length != configuredKey.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedKey),
            Encoding.UTF8.GetBytes(configuredKey));
    }
}
