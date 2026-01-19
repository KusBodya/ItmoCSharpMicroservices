namespace Task21.Models;

public class PagedResponseModel<T>
{
    public ICollection<T> Items { get; init; } = new List<T>();

    public string? PageToken { get; init; }

    public bool HasNextPage => !string.IsNullOrEmpty(PageToken);
}