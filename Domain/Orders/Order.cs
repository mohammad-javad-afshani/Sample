using Domain.Customers;

namespace Domain.Orders;

public class Order
{
    public OrderId Id { get; private set; }
    public CustomerId? CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public List<OrderLine> Lines { get; private set; } = new();
    public decimal TotalAmount => Lines.Sum(l => l.LineTotal);
    public DateTime CreatedAtUtc { get; private set; }

    private Order() { }

    public Order(CustomerId? customerId)
    {
        Id = new OrderId(Guid.NewGuid());
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void AddLine(OrderLine line)
    {
        Lines.Add(line);
    }

    public void MarkStockReserved()
    {
        Status = OrderStatus.StockReserved;
    }

    public void MarkPaid()
    {
        Status = OrderStatus.Paid;
    }
}
