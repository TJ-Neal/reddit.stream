using MediatR;
using Neal.Reddit.Application.Events.Notifications;
using Neal.Reddit.Client.Simple.Interfaces;

namespace Neal.Reddit.Client.Simple.Events.Handlers;

/// <summary>
/// Client handler for the Simple repository for when a record event is raised.
/// </summary>
public class SimpleRecordReceivedHandler : INotificationHandler<KafkaRecordReceivedNotification>
{
    #region Fields

    private readonly ISimpleProducerWrapper recordRepositoryProducerWrapper;

    #endregion Fields

    public SimpleRecordReceivedHandler(ISimpleProducerWrapper recordRepositoryProducer) =>
        this.recordRepositoryProducerWrapper = recordRepositoryProducer;

    #region INotificationHandler Implementation

    public async Task Handle(KafkaRecordReceivedNotification notification, CancellationToken cancellationToken) =>
        await this.recordRepositoryProducerWrapper.ProduceAsync(notification.Record, cancellationToken);

    #endregion INotificationHandler Implementation

    public override int GetHashCode() => base.GetHashCode();
}