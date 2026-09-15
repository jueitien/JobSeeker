using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

[assembly: LambdaSerializer(
    typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobSeeker.NotificationProcessorLambda;

/// <summary>
/// Task #2 queue processor.
/// Amazon SQS triggers this Lambda. Each queued notification is published to
/// Amazon SNS for external delivery such as email.
/// </summary>
public sealed class Function
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IAmazonSimpleNotificationService _sns;
    private readonly string _topicArn;

    public Function()
        : this(new AmazonSimpleNotificationServiceClient())
    {
    }

    internal Function(IAmazonSimpleNotificationService sns)
    {
        _sns = sns;
        _topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")?.Trim()
            ?? string.Empty;
    }

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        if (string.IsNullOrWhiteSpace(_topicArn))
        {
            throw new InvalidOperationException("SNS_TOPIC_ARN is not configured.");
        }

        foreach (var record in sqsEvent.Records)
        {
            NotificationEvent notification;
            try
            {
                notification = JsonSerializer.Deserialize<NotificationEvent>(
                    record.Body,
                    JsonOptions)
                    ?? throw new JsonException("Notification payload was empty.");
            }
            catch (JsonException exception)
            {
                context.Logger.LogError(
                    $"SQS message {record.MessageId} contains invalid JSON: {exception.Message}");
                throw;
            }

            if (string.IsNullOrWhiteSpace(notification.NotificationType) ||
                string.IsNullOrWhiteSpace(notification.Title) ||
                string.IsNullOrWhiteSpace(notification.Message))
            {
                context.Logger.LogError(
                    $"SQS message {record.MessageId} is missing required notification fields.");
                throw new InvalidOperationException("Queued notification is invalid.");
            }

            var createdAtUtc = notification.CreatedAtUtc == default
                ? DateTime.UtcNow
                : notification.CreatedAtUtc;

            var snsMessage =
                $"JobSeeker Administrator Alert\n" +
                $"================================\n\n" +
                $"Type: {notification.NotificationType}\n" +
                $"Reference: {notification.ReferenceType ?? "N/A"} #{notification.ReferenceId?.ToString() ?? "N/A"}\n" +
                $"Created (UTC): {createdAtUtc:yyyy-MM-dd HH:mm:ss}\n\n" +
                notification.Message;

            var publishResponse = await _sns.PublishAsync(new PublishRequest
            {
                TopicArn = _topicArn,
                Subject = TruncateSubject(notification.Title),
                Message = snsMessage
            });

            context.Logger.LogInformation(
                $"Processed SQS message {record.MessageId}; published {notification.NotificationType} to SNS. SNS MessageId={publishResponse.MessageId}");
        }
    }

    private static string TruncateSubject(string subject)
    {
        var clean = subject.Trim();
        return clean.Length <= 100 ? clean : clean[..100];
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
