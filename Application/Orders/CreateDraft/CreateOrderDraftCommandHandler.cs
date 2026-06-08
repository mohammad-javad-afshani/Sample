using Application.Data;
using Domain.Orders;
using Domain.Products;
using MediatR;

namespace Application.Orders.CreateDraft;

internal sealed class CreateOrderDraftCommandHandler : IRequestHandler<CreateOrderDraftCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderDraftCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateOrderDraftCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.FindByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            throw new ProductNotFoundExeption(request.ProductId);
        }

        var order = new Order(request.CustomerId);
        order.AddLine(new OrderLine(request.ProductId, request.Quantity, product.Price));

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id.Value;
    }
}
