using Domain.Customers;
using Domain.Orders;
using Domain.Products;
using MediatR;

namespace Application.Orders.CreateDraft;

public record CreateOrderDraftCommand(
    CustomerId? CustomerId,
    ProductId ProductId,
    int Quantity) : IRequest<Guid>;
