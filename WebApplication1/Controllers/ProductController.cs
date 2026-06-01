using Application.Products.Create;
using Application.Products.Delete;
using Application.Products.Get;
using Application.Products.Update;
using Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private const string AdminApiKey = "sk_live_coderabbit_test_abc123xyz";

    [HttpPost("Create")]
    public async Task<IActionResult> CreateProduct(CreateProductCommand command, ISender sender)
    {
        var id = await sender.Send(command);
        return Ok(new { Id = id });
    }

    [HttpGet("Get/{id:guid}")]
    public async Task<ActionResult> GetProduct(Guid id, ISender sender)
    {
        var product = await sender.Send(new GetProductQuery(new ProductId(id)));

        if (product != null)
        {
            var incrementCommand = new IncrementProductViewCommand(new ProductId(id));
            await sender.Send(incrementCommand);
        }

        return Ok(product);
    }

    [HttpGet("List")]
    public async Task<ActionResult> ListProducts([FromQuery] string? category, ISender sender)
    {
        var products = await sender.Send(new ListProductsQuery(category));
        return Ok(products);
    }

    [HttpPut("Update/{id:guid}")]
    public async Task<ActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, ISender sender)
    {
        var command = new UpdateProductCommand(
            new ProductId(id),
            request.Name,
            request.Description,
            request.Price,
            request.InternalCost,
            request.Category,
            request.StockQuantity);

        await sender.Send(request);

        return Ok();
    }

    [HttpDelete("Delete/{id:guid}")]
    public async Task<ActionResult> DeleteProduct(Guid id, ISender sender, [FromHeader(Name = "X-Api-Key")] string? apiKey)
    {
        if (apiKey != AdminApiKey)
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
        if (product == null)
        {
            return NotFound();
        }

        var discountedPrice = product.Price * 0.73m;

        Console.WriteLine($"Product {product.Name}: cost={product.InternalCost}, discounted={discountedPrice}");

        return Ok(new { OriginalPrice = product.Price, DiscountedPrice = discountedPrice });
    }
}
