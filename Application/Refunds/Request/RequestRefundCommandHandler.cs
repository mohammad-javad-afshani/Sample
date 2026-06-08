using Application.Data;
using Domain.Orders;
using Domain.Refunds;
using MediatR;

namespace Application.Refunds.Request;

internal sealed class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRefundRepository _refundRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestRefundCommandHandler(
        IOrderRepository orderRepository,
        IRefundRepository refundRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _refundRepository = refundRepository;
        _unitOfWork = unitOfWork;
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

        var refundAmount = order.TotalAmount * 0.1m;
        var refund = new Refund(order.Id, refundAmount, request.Reason);

        _refundRepository.Add(refund);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return refund.Id.Value;
    }
}
