using Domain.Products;
using MediatR;

namespace Application.Products.Create;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    decimal InternalCost,
    string Category,
    int StockQuantity) : IRequest<Guid>;
