namespace Application.Ports.Filters;

public record ProductSearchFilter
{
    public IReadOnlyCollection<long>? ProductIds { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public string? NameSubstring { get; init; }
}
