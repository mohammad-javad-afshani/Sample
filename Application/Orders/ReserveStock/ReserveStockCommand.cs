using Domain.Orders;
using Domain.Products;
using MediatR;

namespace Application.Orders.ReserveStock;

public record ReserveStockCommand(OrderId OrderId) : IRequest;
