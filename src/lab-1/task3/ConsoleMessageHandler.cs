using System.Text;

namespace Task3;

public sealed class ConsoleMessageHandler : IMessageHandler
{
    public ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        var messagesList = messages.ToList();

        if (messagesList.Count == 0)
            return ValueTask.CompletedTask;

        var sB = new StringBuilder();
        sB.AppendLine($"[Batch of {messagesList.Count} messages]");

        foreach (Message? message in messagesList)
        {
            sB.AppendLine($"  Title: {message.Title}");
            sB.AppendLine($"  Text: {message.Text}");
            sB.AppendLine();
        }

        Console.WriteLine(sB.ToString());

        return ValueTask.CompletedTask;
    }
}