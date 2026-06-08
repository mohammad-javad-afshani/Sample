using Domain.Orders;
using Domain.Payments;
using MediatR;

namespace Application.Notifications;

public record NotifyPaymentCompletedCommand(OrderId OrderId, PaymentId PaymentId, decimal Amount)
    : IRequest;
