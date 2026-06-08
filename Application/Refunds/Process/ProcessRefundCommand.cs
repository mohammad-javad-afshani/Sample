using Domain.Refunds;
using MediatR;

namespace Application.Refunds.Process;

public record ProcessRefundCommand(RefundId RefundId) : IRequest;
