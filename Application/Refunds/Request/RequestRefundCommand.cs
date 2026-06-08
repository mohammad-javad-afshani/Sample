using Domain.Orders;
using MediatR;

namespace Application.Refunds.Request;

public record RequestRefundCommand(OrderId OrderId, string Reason) : IRequest<Guid>;
