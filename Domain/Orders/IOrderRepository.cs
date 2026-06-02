namespace Domain.Orders;

public interface IOrderRepository
{
    void Add(Order order);
    Task<Order?> FindByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    void Update(Order order);
}
