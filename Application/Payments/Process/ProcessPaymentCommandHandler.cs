using Application.Data;
using Application.Notifications;
using Domain.Orders;
using Domain.Payments;
using MediatR;

namespace Application.Payments.Process;

internal sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly IPaymentGatewayClient _paymentGateway;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public ProcessPaymentCommandHandler(
        IPaymentGatewayClient paymentGateway,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ISender sender)
    {
        _paymentGateway = paymentGateway;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.FindByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status != OrderStatus.StockReserved)
        {
            throw new InvalidOperationException("Payment is only allowed after stock has been reserved.");
        }

        var payment = new Payment(order.Id, order.TotalAmount);
        _paymentRepository.Add(payment);

        var gatewayResult = await _paymentGateway.CreatePaymentAsync(
            new PaymentRequest(order.Id.Value, order.TotalAmount, "USD"),
            cancellationToken);

        if (!gatewayResult.Succeeded)
        {
            payment.MarkFailed();
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Payment gateway declined the transaction.");
        }

        payment.MarkCompleted(gatewayResult.ExternalReference);
        order.MarkPaid();

        _paymentRepository.Update(payment);
        _orderRepository.Update(order);

        await _sender.Send(
            new NotifyPaymentCompletedCommand(order.Id, payment.Id, order.PayableAmount),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.Id.Value;
    }
}
