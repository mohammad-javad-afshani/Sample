namespace Domain.Refunds;

public interface IRefundRepository
{
    void Add(Refund refund);
    Task<Refund?> FindByIdAsync(RefundId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Refund>> ListAllAsync(CancellationToken cancellationToken = default);
    void Update(Refund refund);
}
