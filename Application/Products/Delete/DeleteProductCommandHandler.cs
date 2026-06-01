using Application.Data;
using Domain.Products;
using MediatR;

namespace Application.Products.Delete;

internal sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.FindByIdAsync(request.ProductId);
            if (product == null)
            {
                return;
            }

            _productRepository.Delete(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return;
        }
    }
}
