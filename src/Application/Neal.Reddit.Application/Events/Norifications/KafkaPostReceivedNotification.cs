using MediatR;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Application.Events.Notifications;

/// <summary>
/// Represents a notification for when a post has been received.
/// </summary>
/// <param name="Post"></param>
public record KafkaPostReceivedNotification(Link Post) : INotification;
