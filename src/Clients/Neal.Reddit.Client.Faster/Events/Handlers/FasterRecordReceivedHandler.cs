using MediatR;
using Neal.Reddit.Application.Events.Notifications;
using Neal.Reddit.Client.Faster.Interfaces;

namespace Neal.Reddit.Client.Faster.Events.Handlers;

/// <summary>
/// Client handler for the FasterKV repository for when a record event is raised.
/// </summary>
public class FasterRecordReceivedHandler : INotificationHandler<KafkaPostReceivedNotification>
{
    private readonly IFasterProducerWrapper recordRepositoryProducerWrapper;

    public FasterRecordReceivedHandler(IFasterProducerWrapper recordRepositoryProducer) =>
        this.recordRepositoryProducerWrapper = recordRepositoryProducer;

    public async Task Handle(KafkaPostReceivedNotification notification, CancellationToken cancellationToken) =>
        await this.recordRepositoryProducerWrapper
            .ProduceAsync(notification.Post, cancellationToken);

    public override int GetHashCode() => base.GetHashCode();
}
