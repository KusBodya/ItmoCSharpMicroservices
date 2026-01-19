namespace Kafka.Consumer;

public interface IMessageHandler<TKey, TValue>
{
    Task HandleAsync(IReadOnlyCollection<MessageContext<TKey, TValue>> messages, CancellationToken cancellationToken);
}

public record MessageContext<TKey, TValue>(TKey Key, TValue Value, long Offset, int Partition);
