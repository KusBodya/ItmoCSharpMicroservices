namespace Application41.Ports.Kafka;

public interface IOrderCreationProducer
{
    Task PublishOrderCreatedAsync(long orderId, DateTime createdAt, CancellationToken cancellationToken = default);

    Task PublishOrderProcessingStartedAsync(long orderId, DateTime startedAt, CancellationToken cancellationToken = default);
}
