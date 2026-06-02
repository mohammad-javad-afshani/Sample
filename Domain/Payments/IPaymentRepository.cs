namespace Domain.Payments;

public interface IPaymentRepository
{
    void Add(Payment payment);
    Task<Payment?> FindByIdAsync(PaymentId id, CancellationToken cancellationToken = default);
    void Update(Payment payment);
}
