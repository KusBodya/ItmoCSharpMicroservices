namespace Task3;

public class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("===  Testing  ===\n");

        var options = new MessageProcessorOptions
        {
            ChannelCapacity = 50,
            BatchSize = 5,
            BatchTimeout = TimeSpan.FromMilliseconds(200),
        };

        var implementation = new MessageProcessor(
            handlers: [new ConsoleMessageHandler()],
            options: options);

        MessageProcessor processor = implementation;
        MessageProcessor sender = implementation;

        Task processingTask = processor.ProcessAsync(CancellationToken.None);

        int messageCount = 47;
        await Parallel.ForEachAsync(
            Enumerable.Range(1, messageCount),
            new ParallelOptions { MaxDegreeOfParallelism = 4 },
            async (i, ct) =>
            {
                var message = new Message(
                    Title: $"Message #{i}",
                    Text: $"This is message number {i} from thread {Environment.CurrentManagedThreadId}");

                await sender.SendAsync(message, ct);
                Console.WriteLine($"Sent message #{i}");

                await Task.Delay(50, ct);
            });

        Console.WriteLine("\n=== Все сообщения отправлены ===\n");

        processor.Complete();

        await processingTask;

        Console.WriteLine("\n=== Обработка завершена ===");
    }
}