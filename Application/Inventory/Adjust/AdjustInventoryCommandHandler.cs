using Application.Data;
using Domain.Products;
using MediatR;

namespace Application.Inventory.Adjust;

internal sealed class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustInventoryCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.FindByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            throw new ProductNotFoundExeption(request.ProductId);
        }

        product.StockQuantity = product.StockQuantity + request.Delta;
        _productRepository.Update(product);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
