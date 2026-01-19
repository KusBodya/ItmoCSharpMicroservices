using Google.Protobuf;
using Kafka.Configuration;
using Kafka.Consumer;
using Kafka.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaProducer<TKey, TValue>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        services.Configure<KafkaProducerOptions>(configuration.GetSection(sectionName));
        services.AddSingleton<IKafkaProducer<TKey, TValue>, KafkaProducer<TKey, TValue>>();

        return services;
    }

    public static IServiceCollection AddKafkaConsumer<TKey, TValue, THandler>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
        where THandler : class, IMessageHandler<TKey, TValue>
    {
        services.Configure<KafkaConsumerOptions>(configuration.GetSection(sectionName));
        services.AddSingleton<IMessageHandler<TKey, TValue>, THandler>();
        services.AddHostedService<KafkaConsumerService<TKey, TValue>>();

        return services;
    }
}
