using Domain41.Enums;

namespace Application41.Ports.Filters;

public record OrderSearchFilter
{
    public IReadOnlyCollection<long>? OrderIds { get; set; }

    public OrderState? State { get; set; }

    public string? Author { get; set; }
}