using Application.Products.Create;
using Application.Products.Delete;
using Application.Products.Get;
using Application.Products.Increment;
using Application.Products.QuickCreate;
using Application.Products.Update;
using Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private const string AdminApiKeyConfigPath = "ProductApi:AdminApiKey";
    private const string DiscountRateConfigPath = "ProductApi:DiscountRate";

    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IConfiguration configuration, ILogger<ProductController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProduct(CreateProductCommand command, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var id = await sender.Send(command);
        return Ok(new { Id = id });
    }

    [HttpPost("QuickCreate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> QuickCreateProduct(QuickCreateProductCommand command, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var id = await sender.Send(command);
        return Ok(new { Id = id });
    }

    [HttpGet("Get/{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> GetProduct(Guid id, ISender sender)
    {
        var product = await sender.Send(new GetProductQuery(new ProductId(id)));
        return Ok(product);
    }

    [HttpPost("View/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> IncrementViewCount(Guid id, ISender sender)
    {
        await sender.Send(new IncrementProductViewCommand(new ProductId(id)));
        return Ok();
    }

    [HttpGet("Detail/{id:guid}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDetailResponse>> GetProductDetail(Guid id, ISender sender)
    {
        var detail = await sender.Send(new GetProductDetailQuery(new ProductId(id)));
        return Ok(detail);
    }

    [HttpGet("Catalog")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductCatalogItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductCatalogItemResponse>>> GetProductCatalog(ISender sender)
    {
        var catalog = await sender.Send(new GetProductCatalogQuery());
        return Ok(catalog);
    }

    [HttpGet("List")]
    [ProducesResponseType(typeof(PagedProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedProductResponse>> ListProducts(
        ISender sender,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var products = await sender.Send(new ListProductsQuery(category, page, pageSize));
        return Ok(products);
    }

    [HttpPut("Update/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetDiscountedPrice(Guid id, ISender sender)
    {
        var product = await sender.Send(new GetProductQuery(new ProductId(id)));
        var discountRate = _configuration.GetValue<decimal>(DiscountRateConfigPath, 0.73m);

        if (discountRate is <= 0 or > 1)
        {
            return BadRequest("Invalid discount rate configuration.");
        }

        var discountedPrice = Math.Round(product.Price * discountRate, 2, MidpointRounding.AwayFromZero);

        _logger.LogInformation("Calculated discounted price for product {ProductId}", id);

        return Ok(new { OriginalPrice = product.Price, DiscountedPrice = discountedPrice });
    }

    private bool IsAuthorized()
    {
        var configuredKey = _configuration[AdminApiKeyConfigPath];
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
