using Application.Inventory.Adjust;
using Application.Refunds.List;
using Application.Refunds.Process;
using Application.Refunds.Request;
using Domain.Orders;
using Domain.Products;
using Domain.Refunds;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class RefundController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public RefundController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("List")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRefunds(ISender sender)
    {
        var refunds = await sender.Send(new ListRefundsQuery());
        return Ok(refunds);
    }

    [HttpPost("Request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RequestRefund([FromBody] RequestRefundBody body, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var id = await sender.Send(new RequestRefundCommand(new OrderId(body.OrderId), body.Reason));
        return Ok(new { RefundId = id });
    }

    [HttpPost("{refundId:guid}/Process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessRefund(Guid refundId, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        await sender.Send(new ProcessRefundCommand(new RefundId(refundId)));
        return Ok();
    }

    [HttpPost("Inventory/Adjust")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdjustInventory([FromBody] AdjustInventoryBody body, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        await sender.Send(new AdjustInventoryCommand(new ProductId(body.ProductId), body.Delta));
        return Ok();
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

public record RequestRefundBody(Guid OrderId, string Reason);

public record AdjustInventoryBody(Guid ProductId, int Delta);
