using MediatR;

namespace Application.Refunds.List;

public record ListRefundsQuery : IRequest<IReadOnlyList<RefundListItem>>;

public record RefundListItem(Guid Id, Guid OrderId, decimal Amount, string Reason, string Status);
