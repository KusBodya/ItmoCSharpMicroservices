namespace Kafka.Configuration;

public class KafkaProducerOptions
{
    public string BootstrapServers { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public int? MessageTimeoutMs { get; set; }

    public int? RequestTimeoutMs { get; set; }
}
