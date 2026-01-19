namespace Task23.Abstractions;

public interface IContentRenderer
{
    Task RenderAsync(CancellationToken cancellationToken = default);
}
