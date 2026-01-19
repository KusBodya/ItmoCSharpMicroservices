using Domain.Enums;

namespace Application.Ports.Filters;

public record OrderHistorySearchFilter
{
    public IReadOnlyCollection<long>? OrderIds { get; set; }

    public OrderHistoryItemKind? HistoryKind { get; set; }
}