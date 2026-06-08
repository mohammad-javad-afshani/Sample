using Application.Orders.CreateDraft;
using Application.Orders.ReserveStock;
using Application.Payments.Process;
using Domain.Customers;
using Domain.Orders;
using Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

/// <summary>
/// Checkout workflow: draft order → reserve stock → capture payment.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CommerceController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public CommerceController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("Orders/Draft")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrderDraft(
        [FromBody] CreateOrderDraftRequest body,
        ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var orderId = await sender.Send(new CreateOrderDraftCommand(
            body.CustomerId.HasValue ? new CustomerId(body.CustomerId.Value) : null,
            new ProductId(body.ProductId),
            body.Quantity));

        return Ok(new { OrderId = orderId });
    }

    [HttpPost("Orders/{orderId:guid}/ReserveStock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReserveStock(Guid orderId, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        await sender.Send(new ReserveStockCommand(new OrderId(orderId)));
        return Ok();
    }

    [HttpPost("Orders/{orderId:guid}/Pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessPayment(Guid orderId, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var paymentId = await sender.Send(new ProcessPaymentCommand(new OrderId(orderId)));
        return Ok(new { PaymentId = paymentId });
    }

    private bool IsAuthorized() => ApiKeyAuth.IsAuthorized(Request, _configuration);
}

public record CreateOrderDraftRequest(Guid? CustomerId, Guid ProductId, int Quantity);
