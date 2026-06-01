using Application.Data;
using Domain.Products;
using MediatR;

namespace Application.Products.Get;

public record IncrementProductViewCommand(ProductId ProductId) : IRequest;

internal sealed class IncrementProductViewCommandHandler : IRequestHandler<IncrementProductViewCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IncrementProductViewCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(IncrementProductViewCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.FindByIdAsync(request.ProductId);
        if (product == null)
        {
            return;
        }

        product.IncrementViewCount();
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
