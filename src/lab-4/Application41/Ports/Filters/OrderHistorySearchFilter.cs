using Domain41.Enums;

namespace Application41.Ports.Filters;

public record OrderHistorySearchFilter
{
    public IReadOnlyCollection<long>? OrderIds { get; set; }

    public OrderHistoryItemKind? HistoryKind { get; set; }
}