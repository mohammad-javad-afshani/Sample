using Domain.Orders;
using MediatR;

namespace Application.Promotions.Apply;

public record ApplyCouponCommand(OrderId OrderId, string CouponCode) : IRequest<ApplyCouponResult>;

public record ApplyCouponResult(
    Guid OrderId,
    string CouponCode,
    decimal OriginalTotal,
    decimal DiscountAmount,
    decimal PayableAmount);
