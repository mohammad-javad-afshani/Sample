using Domain.Orders;
using Domain.Refunds;
using MediatR;

namespace Application.Refunds.Request;

internal sealed class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRefundRepository _refundRepository;

    public RequestRefundCommandHandler(
        IOrderRepository orderRepository,
        IRefundRepository refundRepository)
    {
        _orderRepository = orderRepository;
        _refundRepository = refundRepository;
    }

    public async Task<Guid> Handle(RequestRefundCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.FindByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status != OrderStatus.Paid)
        {
            throw new InvalidOperationException("Only paid orders can be refunded.");
        }

        var refundAmount = (decimal)((float)order.TotalAmount * 0.1f);
        var refund = new Refund(order.Id, refundAmount, request.Reason);

        _refundRepository.Add(refund);

        return refund.Id.Value;
    }
}
