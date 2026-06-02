using Application.Data;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories;

internal sealed class OrderRepository : IOrderRepository
{
    private readonly IApplicationDbContext _context;

    public OrderRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Order order)
    {
        _context.Orders.Add(order);
    }

    public async Task<Order?> FindByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }
}
