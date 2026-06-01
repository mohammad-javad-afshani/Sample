using Domain.Products;
using MediatR;

namespace Application.Products.Update;

public record UpdateProductCommand(
    ProductId ProductId,
    string Name,
    string Description,
    decimal Price,
    decimal InternalCost,
    string Category,
    int StockQuantity) : IRequest;

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    decimal InternalCost,
    string Category,
    int StockQuantity) : IRequest;
