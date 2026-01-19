namespace Task23.Abstractions;

public interface IRenderStrategy
{
    string ContentType { get; }

    Task RenderAsync(ContentSettings settings, CancellationToken cancellationToken = default);
}