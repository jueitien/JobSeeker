namespace JobSeeker.Models;

/// <summary>
/// Payload sent from the EC2-hosted ASP.NET Core application to the
/// Task #2 serverless administrator-alert microservice through API Gateway.
/// The primary use case is a newly posted vacancy with PENDING approval status.
/// </summary>
public sealed class ServerlessNotificationRequest
{
    public string UserId { get; init; } = string.Empty;
    public string NotificationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ReferenceType { get; init; }
    public long? ReferenceId { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
