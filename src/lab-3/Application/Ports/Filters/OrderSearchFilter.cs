using Domain.Enums;

namespace Application.Ports.Filters;

public record OrderSearchFilter
{
    public IReadOnlyCollection<long>? OrderIds { get; set; }

    public OrderState? State { get; set; }

    public string? Author { get; set; }
}