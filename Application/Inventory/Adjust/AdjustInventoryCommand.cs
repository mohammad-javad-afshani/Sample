using Domain.Products;
using MediatR;

namespace Application.Inventory.Adjust;

public record AdjustInventoryCommand(ProductId ProductId, int Delta) : IRequest;
