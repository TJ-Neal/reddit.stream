using MediatR;
using Neal.Reddit.Core.Entities.Reddit;

namespace Neal.Reddit.Application.Events.Notifications;

/// <summary>
/// Represents a notification for when a record has been received.
/// </summary>
/// <param name="Record"></param>
public record KafkaRecordReceivedNotification(DataBase Record) : INotification;
