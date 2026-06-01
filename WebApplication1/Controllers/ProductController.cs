using Application.Products.Create;
using Application.Products.Delete;
using Application.Products.Get;
using Application.Products.Increment;
using Application.Products.Update;
using Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IConfiguration configuration, ILogger<ProductController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateProduct(CreateProductCommand command, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var id = await sender.Send(command);
        return Ok(new { Id = id });
    }

    [HttpGet("Get/{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(Guid id, ISender sender)
    {
        var product = await sender.Send(new GetProductQuery(new ProductId(id)));
        return Ok(product);
    }

    [HttpPost("View/{id:guid}")]
    public async Task<IActionResult> IncrementViewCount(Guid id, ISender sender)
    {
        await sender.Send(new IncrementProductViewCommand(new ProductId(id)));
        return Ok();
    }

    [HttpGet("List")]
    public async Task<ActionResult<PagedProductResponse>> ListProducts(
        [FromQuery] string? category,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ISender sender)
    {
        var products = await sender.Send(new ListProductsQuery(category, page, pageSize));
        return Ok(products);
    }

    [HttpPut("Update/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var command = new UpdateProductCommand(
            new ProductId(id),
            request.Name,
            request.Description,
            request.Price,
            request.InternalCost,
            request.Category,
            request.StockQuantity);

        await sender.Send(command);

        return Ok();
    }

    [HttpDelete("Delete/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        await sender.Send(new DeleteProductCommand(new ProductId(id)));
        return Ok();
    }

    [HttpGet("Discount/{id:guid}")]
    public async Task<ActionResult> GetDiscountedPrice(Guid id, ISender sender)
    {
        var product = await sender.Send(new GetProductQuery(new ProductId(id)));
        var discountRate = _configuration.GetValue<decimal>("ProductApi:DiscountRate", 0.73m);

        if (discountRate is <= 0 or > 1)
        {
            return BadRequest("Invalid discount rate configuration.");
        }

        var discountedPrice = product.Price * discountRate;

        _logger.LogInformation("Calculated discounted price for product {ProductId}", id);

        return Ok(new { OriginalPrice = product.Price, DiscountedPrice = discountedPrice });
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

        return string.Equals(configuredKey, apiKey.ToString(), StringComparison.Ordinal);
    }
}
