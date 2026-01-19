namespace Application.Ports.Filters;

public record OrderItemSearchFilter
{
    public IReadOnlyCollection<long>? OrderIds { get; init; }

    public IReadOnlyCollection<long>? ProductIds { get; init; }

    public bool? IsDeleted { get; init; }
}
