using System.Threading.Channels;

namespace Task3;

public sealed class MessageProcessor : IMessageProcessor, IMessageSender
{
    private readonly Channel<Message> _channel;
    private readonly IReadOnlyCollection<IMessageHandler> _handlers;
    private readonly int _batchSize;
    private readonly TimeSpan _batchTimeout;

    public MessageProcessor(
        IEnumerable<IMessageHandler> handlers,
        MessageProcessorOptions? options = null)
    {
        options ??= new MessageProcessorOptions();

        _handlers = handlers.ToList();
        _batchSize = options.BatchSize;
        _batchTimeout = options.BatchTimeout;

        var channelOptions = new BoundedChannelOptions(options.ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        };

        _channel = Channel.CreateBounded<Message>(channelOptions);
    }

    public async ValueTask SendAsync(Message message, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public void Complete()
    {
        _channel.Writer.Complete();
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (IReadOnlyList<Message> batch in _channel.Reader
                           .ReadAllAsync(cancellationToken)
                           .ChunkAsync(_batchSize, _batchTimeout)
                           .WithCancellation(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            foreach (IMessageHandler handler in _handlers)
            {
                await handler.HandleAsync(batch, cancellationToken);
            }
        }
    }
}