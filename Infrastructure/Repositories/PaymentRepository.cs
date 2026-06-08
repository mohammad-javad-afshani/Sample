using Application.Data;
using Domain.Orders;
using Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

internal sealed class PaymentRepository : IPaymentRepository
{
    private readonly IApplicationDbContext _context;

    public PaymentRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Payment payment)
    {
        _context.Payments.Add(payment);
    }

    public async Task<Payment?> FindByIdAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> FindCompletedByOrderIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(
                p => p.OrderId == orderId && p.Status == PaymentStatus.Completed,
                cancellationToken);
    }

    public void Update(Payment payment)
    {
        _context.Payments.Update(payment);
    }
}
