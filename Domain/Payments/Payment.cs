using Domain.Orders;

namespace Domain.Payments;

public class Payment
{
    public PaymentId Id { get; private set; }
    public OrderId OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string? ExternalReference { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Payment() { }

    public Payment(OrderId orderId, decimal amount)
    {
        Id = new PaymentId(Guid.NewGuid());
        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted(string externalReference)
    {
        ExternalReference = externalReference;
        Status = PaymentStatus.Completed;
    }

    public void MarkFailed()
    {
        Status = PaymentStatus.Failed;
    }
}
