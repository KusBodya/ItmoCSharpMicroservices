using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Configuration;
using Kafka.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Producer;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>, IDisposable
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly KafkaProducerOptions _options;
    private readonly ILogger<KafkaProducer<TKey, TValue>> _logger;

    public KafkaProducer(
        IOptions<KafkaProducerOptions> options,
        ILogger<KafkaProducer<TKey, TValue>> logger)
    {
        _options = options.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            MessageTimeoutMs = _options.MessageTimeoutMs,
            RequestTimeoutMs = _options.RequestTimeoutMs,
        };

        _producer = new ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(new ProtobufSerializer<TKey>())
            .SetValueSerializer(new ProtobufSerializer<TValue>())
            .Build();
    }

    public async Task ProduceAsync(TKey key, TValue value, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<TKey, TValue>
            {
                Key = key,
                Value = value,
            };

            DeliveryResult<TKey, TValue> result = await _producer.ProduceAsync(
                _options.Topic,
                message,
                cancellationToken);

            _logger.LogInformation(
                "Message delivered to {Topic} [{Partition}] at offset {Offset}",
                result.Topic,
                result.Partition.Value,
                result.Offset.Value);
        }
        catch (ProduceException<TKey, TValue> ex)
        {
            _logger.LogError(ex, "Failed to deliver message to {Topic}", _options.Topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
