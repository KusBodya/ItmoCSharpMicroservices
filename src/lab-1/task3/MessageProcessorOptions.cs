namespace Task3;

public sealed class MessageProcessorOptions
{
    public int ChannelCapacity { get; set; } = 100;

    public int BatchSize { get; set; } = 10;

    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(1);
}