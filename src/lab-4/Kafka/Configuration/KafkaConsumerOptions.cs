namespace Kafka.Configuration;

public class KafkaConsumerOptions
{
    public string BootstrapServers { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public string GroupId { get; set; } = string.Empty;

    public int BatchSize { get; set; } = 10;

    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public bool EnableAutoCommit { get; set; } = false;

    public string AutoOffsetReset { get; set; } = "earliest";
}
