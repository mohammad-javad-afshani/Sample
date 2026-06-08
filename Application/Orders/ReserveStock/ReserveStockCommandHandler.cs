using Application.Data;
using Domain.Orders;
using Domain.Products;
using MediatR;

namespace Application.Orders.ReserveStock;

internal sealed class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReserveStockCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.FindByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        foreach (var line in order.Lines)
        {
            var product = await _productRepository.FindByIdAsync(line.ProductId, cancellationToken);
            if (product is null)
            {
                throw new ProductNotFoundExeption(line.ProductId);
            }

            product.ReserveStock(line.Quantity);
            _productRepository.Update(product);
        }

        order.MarkStockReserved();
        _orderRepository.Update(order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
