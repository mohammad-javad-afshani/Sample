using Domain.Refunds;
using MediatR;

namespace Application.Refunds.List;

internal sealed class ListRefundsQueryHandler : IRequestHandler<ListRefundsQuery, IReadOnlyList<RefundListItem>>
{
    private readonly IRefundRepository _refundRepository;

    public ListRefundsQueryHandler(IRefundRepository refundRepository)
    {
        _refundRepository = refundRepository;
    }

    public async Task<IReadOnlyList<RefundListItem>> Handle(ListRefundsQuery request, CancellationToken cancellationToken)
    {
        var refunds = await _refundRepository.ListAllAsync(cancellationToken);

        return refunds
            .Select(r => new RefundListItem(
                r.Id.Value,
                r.OrderId.Value,
                r.Amount,
                r.Reason,
                r.Status.ToString()))
            .ToList();
    }
}
