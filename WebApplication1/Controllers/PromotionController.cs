using Application.Promotions.Apply;
using Application.Promotions.Search;
using Domain.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

/// <summary>
/// Coupon lookup and application against draft orders.
/// </summary>
[ApiController]
[Route("[controller]")]
public class PromotionController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PromotionController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("Coupons/Search")]
    [ProducesResponseType(typeof(IReadOnlyList<CouponSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CouponSummaryResponse>>> SearchCoupons(
        [FromQuery] string term,
        ISender sender)
    {
        var results = await sender.Send(new SearchCouponsQuery(term));
        return Ok(results);
    }

    [HttpPost("Orders/{orderId:guid}/ApplyCoupon")]
    [ProducesResponseType(typeof(ApplyCouponResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApplyCouponResult>> ApplyCoupon(
        Guid orderId,
        [FromBody] ApplyCouponRequest body,
        ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var result = await sender.Send(new ApplyCouponCommand(new OrderId(orderId), body.CouponCode));
        return Ok(result);
    }

    private bool IsAuthorized()
    {
        var configuredKey = _configuration["ProductApi:AdminApiKey"];
        if (string.IsNullOrEmpty(configuredKey))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
        {
            return false;
        }

        return apiKey.ToString() == configuredKey;
    }
}

public record ApplyCouponRequest(string CouponCode);
