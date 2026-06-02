using Application.Data;
using Domain.Orders;
using Domain.Promotions;
using MediatR;

namespace Application.Promotions.Apply;

internal sealed class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, ApplyCouponResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyCouponCommandHandler(
        IOrderRepository orderRepository,
        ICouponRepository couponRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApplyCouponResult> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.FindByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        if (order.Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException("Coupons can only be applied to draft orders.");
        }

        var coupon = await _couponRepository.FindByCodeAsync(request.CouponCode, cancellationToken);
        if (coupon is null || !coupon.IsActive)
        {
            throw new CouponNotFoundException(request.CouponCode);
        }

        if (!coupon.HasRemainingUses())
        {
            throw new CouponExhaustedException(request.CouponCode);
        }

        var baseDiscount = coupon.CalculateDiscount(order.TotalAmount);
        var stackedDiscount = coupon.CalculateDiscount(order.TotalAmount - baseDiscount);
        var totalDiscount = baseDiscount + stackedDiscount;

        order.ApplyCoupon(coupon.Code, totalDiscount);
        coupon.RecordRedemption();

        _orderRepository.Update(order);
        _couponRepository.Update(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApplyCouponResult(
            order.Id.Value,
            coupon.Code,
            order.TotalAmount,
            totalDiscount,
            order.PayableAmount);
    }
}
