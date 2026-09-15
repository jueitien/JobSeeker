using System.Net.Http.Json;
using JobSeeker.Models;

namespace JobSeeker.Services;

/// <summary>
/// Calls the Task #2 administrator-alert microservice exposed by Amazon API Gateway.
/// The integration is intentionally best-effort: the vacancy is already stored
/// in RDS before this call, so a temporary serverless outage does not prevent an
/// employer from posting a vacancy.
/// </summary>
public sealed class ServerlessNotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServerlessNotificationClient> _logger;

    public ServerlessNotificationClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ServerlessNotificationClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync(
        ServerlessNotificationRequest payload,
        CancellationToken cancellationToken = default)
    {
        if (!bool.TryParse(
                _configuration["ServerlessNotifications:Enabled"],
                out var enabled) || !enabled)
        {
            return;
        }

        var apiUrl = _configuration["ServerlessNotifications:ApiUrl"]?.Trim();
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            _logger.LogWarning(
                "ServerlessNotifications is enabled but ApiUrl is not configured.");
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = JsonContent.Create(payload)
            };

            var apiKey = _configuration["ServerlessNotifications:ApiKey"]?.Trim();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.TryAddWithoutValidation(
                    "X-JobSeeker-Key",
                    apiKey);
            }

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                _logger.LogWarning(
                    "Task #2 notification microservice returned HTTP {StatusCode}. Body: {Body}",
                    (int)response.StatusCode,
                    responseBody);
                return;
            }

            _logger.LogInformation(
                "Notification event {NotificationType} was published through the Task #2 API Gateway microservice.",
                payload.NotificationType);
        }
        catch (Exception exception)
        {
            // Do not break the vacancy-posting workflow when the serverless
            // microservice is unavailable. The PENDING vacancy is already
            // persisted in RDS and can still be reviewed by an administrator.
            _logger.LogWarning(
                exception,
                "Task #2 notification microservice call failed for {NotificationType}.",
                payload.NotificationType);
        }
    }
}
