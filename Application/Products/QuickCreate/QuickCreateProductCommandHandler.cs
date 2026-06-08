using Application.Data;
using Domain.Products;
using MediatR;

namespace Application.Products.QuickCreate;

internal sealed class QuickCreateProductCommandHandler : IRequestHandler<QuickCreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuickCreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(QuickCreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Price);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id.Value;
    }
}
