using Domain.Orders;
using Domain.Payments;
using MediatR;

namespace Application.Payments.Process;

public record ProcessPaymentCommand(OrderId OrderId) : IRequest<Guid>;
