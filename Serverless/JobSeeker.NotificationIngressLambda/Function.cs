using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;

[assembly: LambdaSerializer(
    typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobSeeker.NotificationIngressLambda;

/// <summary>
/// Task #2 ingress microservice.
/// API Gateway invokes this function. The function validates the request and
/// places the notification event into Amazon SQS for asynchronous processing.
/// </summary>
public sealed class Function
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IAmazonSQS _sqs;
    private readonly string _queueUrl;
    private readonly string? _sharedApiKey;

    public Function()
        : this(new AmazonSQSClient())
    {
    }

    internal Function(IAmazonSQS sqs)
    {
        _sqs = sqs;
        _queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL")?.Trim()
            ?? string.Empty;
        _sharedApiKey = Environment.GetEnvironmentVariable("SHARED_API_KEY")?.Trim();
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
        APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        if (string.IsNullOrWhiteSpace(_queueUrl))
        {
            context.Logger.LogError("QUEUE_URL is not configured.");
            return Response(HttpStatusCode.InternalServerError,
                new { error = "SQS queue is not configured." });
        }

        if (!IsAuthorized(request))
        {
            context.Logger.LogWarning(
                "Rejected notification request because the shared API key did not match.");
            return Response(HttpStatusCode.Unauthorized,
                new { error = "Unauthorized." });
        }

        NotificationEvent? notification;
        try
        {
            notification = JsonSerializer.Deserialize<NotificationEvent>(
                request.Body ?? string.Empty,
                JsonOptions);
        }
        catch (JsonException exception)
        {
            context.Logger.LogWarning($"Invalid JSON payload: {exception.Message}");
            return Response(HttpStatusCode.BadRequest,
                new { error = "Invalid JSON payload." });
        }

        if (notification is null ||
            string.IsNullOrWhiteSpace(notification.NotificationType) ||
            string.IsNullOrWhiteSpace(notification.Title) ||
            string.IsNullOrWhiteSpace(notification.Message))
        {
            return Response(HttpStatusCode.BadRequest,
                new { error = "notificationType, title and message are required." });
        }

        var queuedEvent = new NotificationEvent
        {
            UserId = notification.UserId,
            NotificationType = notification.NotificationType.Trim(),
            Title = notification.Title.Trim(),
            Message = notification.Message.Trim(),
            ReferenceType = notification.ReferenceType,
            ReferenceId = notification.ReferenceId,
            CreatedAtUtc = notification.CreatedAtUtc == default
                ? DateTime.UtcNow
                : notification.CreatedAtUtc
        };

        var messageBody = JsonSerializer.Serialize(queuedEvent, JsonOptions);

        var sendResponse = await _sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = messageBody
        });

        context.Logger.LogInformation(
            $"Queued {queuedEvent.NotificationType} in SQS. MessageId={sendResponse.MessageId}");

        return Response(HttpStatusCode.Accepted, new
        {
            status = "queued",
            messageId = sendResponse.MessageId,
            notificationType = queuedEvent.NotificationType
        });
    }

    private bool IsAuthorized(APIGatewayHttpApiV2ProxyRequest request)
    {
        if (string.IsNullOrWhiteSpace(_sharedApiKey))
        {
            // Initial lab testing only. Configure SHARED_API_KEY for the final demo.
            return true;
        }

        if (request.Headers is null)
            return false;

        var suppliedKey = request.Headers
            .FirstOrDefault(header => string.Equals(
                header.Key,
                "x-jobseeker-key",
                StringComparison.OrdinalIgnoreCase))
            .Value;

        return string.Equals(
            suppliedKey,
            _sharedApiKey,
            StringComparison.Ordinal);
    }

    private static APIGatewayHttpApiV2ProxyResponse Response(
        HttpStatusCode statusCode,
        object body)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)statusCode,
            Headers = new Dictionary<string, string>
            {
                ["content-type"] = "application/json"
            },
            Body = JsonSerializer.Serialize(body, JsonOptions)
        };
    }
}

public sealed class NotificationEvent
{
    public string UserId { get; init; } = string.Empty;
    public string NotificationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ReferenceType { get; init; }
    public long? ReferenceId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
