using Application41.Ports.Kafka;
using Google.Protobuf.WellKnownTypes;
using Kafka.Producer;
using Orders.Kafka.Contracts;

namespace Infrastructure41.Kafka;

public class OrderCreationProducer : IOrderCreationProducer
{
    private readonly IKafkaProducer<OrderCreationKey, OrderCreationValue> _producer;

    public OrderCreationProducer(IKafkaProducer<OrderCreationKey, OrderCreationValue> producer)
    {
        _producer = producer;
    }

    public async Task PublishOrderCreatedAsync(
        long orderId,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        var key = new OrderCreationKey { OrderId = orderId };
        var value = new OrderCreationValue
        {
            OrderCreated = new OrderCreationValue.Types.OrderCreated
            {
                OrderId = orderId,
                CreatedAt = Timestamp.FromDateTime(createdAt.ToUniversalTime()),
            },
        };

        await _producer.ProduceAsync(key, value, cancellationToken);
    }

    public async Task PublishOrderProcessingStartedAsync(
        long orderId,
        DateTime startedAt,
        CancellationToken cancellationToken = default)
    {
        var key = new OrderCreationKey { OrderId = orderId };
        var value = new OrderCreationValue
        {
            OrderProcessingStarted = new OrderCreationValue.Types.OrderProcessingStarted
            {
                OrderId = orderId,
                StartedAt = Timestamp.FromDateTime(startedAt.ToUniversalTime()),
            },
        };

        await _producer.ProduceAsync(key, value, cancellationToken);
    }
}