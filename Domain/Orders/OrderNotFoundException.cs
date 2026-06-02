namespace Domain.Orders;

public sealed class OrderNotFoundException : Exception
{
    public OrderNotFoundException(OrderId orderId)
        : base($"Order {orderId.Value} was not found.")
    {
    }
}
