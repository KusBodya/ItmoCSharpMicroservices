using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Configuration;
using Kafka.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Consumer;

public class KafkaConsumerService<TKey, TValue> : BackgroundService
    where TKey : IMessage<TKey>, new()
    where TValue : IMessage<TValue>, new()
{
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly KafkaConsumerOptions _options;
    private readonly IMessageHandler<TKey, TValue> _messageHandler;
    private readonly ILogger<KafkaConsumerService<TKey, TValue>> _logger;

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }

    public KafkaConsumerService(
        IOptions<KafkaConsumerOptions> options,
        IMessageHandler<TKey, TValue> messageHandler,
        ILogger<KafkaConsumerService<TKey, TValue>> logger)
    {
        _options = options.Value;
        _messageHandler = messageHandler;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            EnableAutoCommit = _options.EnableAutoCommit,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_options.AutoOffsetReset, ignoreCase: true),
        };

        _consumer = new ConsumerBuilder<TKey, TValue>(config)
            .SetKeyDeserializer(new ProtobufDeserializer<TKey>())
            .SetValueDeserializer(new ProtobufDeserializer<TValue>())
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_options.Topic);
        _logger.LogInformation("Kafka consumer started for topic {Topic}", _options.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessBatchAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Kafka consumer");
            throw;
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var batch = new List<MessageContext<TKey, TValue>>();
        DateTime startTime = DateTime.UtcNow;

        while (batch.Count < _options.BatchSize &&
               DateTime.UtcNow - startTime < _options.BatchTimeout)
        {
            try
            {
                ConsumeResult<TKey, TValue>? result = _consumer.Consume(
                    TimeSpan.FromMilliseconds(100));

                if (result != null && !result.IsPartitionEOF)
                {
                    var messageContext = new MessageContext<TKey, TValue>(
                        result.Message.Key,
                        result.Message.Value,
                        result.Offset.Value,
                        result.Partition.Value);

                    batch.Add(messageContext);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message from {Topic}", _options.Topic);
            }
        }

        if (batch.Count > 0)
        {
            try
            {
                await _messageHandler.HandleAsync(batch, cancellationToken);

                _consumer.Commit();

                _logger.LogInformation(
                    "Processed batch of {Count} messages from {Topic}",
                    batch.Count,
                    _options.Topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch from {Topic}", _options.Topic);
                throw;
            }
        }
    }
}