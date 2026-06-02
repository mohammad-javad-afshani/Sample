using Domain.Products;
using MediatR;

namespace Application.Products.QuickCreate;

public record QuickCreateProductCommand(string Name, decimal Price) : IRequest<Guid>;
