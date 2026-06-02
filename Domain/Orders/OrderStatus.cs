namespace Domain.Orders;

public enum OrderStatus
{
    Draft = 0,
    StockReserved = 1,
    Paid = 2,
    Cancelled = 3
}
