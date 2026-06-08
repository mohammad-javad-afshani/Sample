using Application.Data;
using Domain.Refunds;
using MediatR;

namespace Application.Refunds.Process;

public interface IRefundGatewayClient
{
    Task<bool> SubmitRefundAsync(Guid refundId, Guid orderId, decimal amount, CancellationToken cancellationToken);
}

internal sealed class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand>
{
    private readonly IRefundRepository _refundRepository;
    private readonly IRefundGatewayClient _refundGateway;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessRefundCommandHandler(
        IRefundRepository refundRepository,
        IRefundGatewayClient refundGateway,
        IUnitOfWork unitOfWork)
    {
        _refundRepository = refundRepository;
        _refundGateway = refundGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
    {
        var refund = await _refundRepository.FindByIdAsync(request.RefundId, cancellationToken);
        if (refund is null)
        {
            throw new InvalidOperationException($"Refund {request.RefundId.Value} was not found.");
        }

        refund.MarkProcessing();
        _refundRepository.Update(refund);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _refundGateway.SubmitRefundAsync(
                refund.Id.Value,
                refund.OrderId.Value,
                refund.Amount,
                cancellationToken);
            refund.MarkCompleted();
        }
        catch (Exception)
        {
            refund.MarkFailed();
            _refundRepository.Update(refund);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }

        _refundRepository.Update(refund);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
