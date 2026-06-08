using Domain.Orders;

namespace Domain.Refunds;

public class Refund
{
    public RefundId Id { get; private set; }
    public OrderId OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Refund() { }

    public Refund(OrderId orderId, decimal amount, string reason)
    {
        Id = new RefundId(Guid.NewGuid());
        OrderId = orderId;
        Amount = amount;
        Reason = reason;
        Status = RefundStatus.Requested;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkProcessing() => Status = RefundStatus.Processing;

    public void MarkCompleted() => Status = RefundStatus.Completed;

    public void MarkFailed() => Status = RefundStatus.Failed;
}
