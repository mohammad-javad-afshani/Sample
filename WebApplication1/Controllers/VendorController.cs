using Application.Vendors.Create;
using Application.Vendors.Export;
using Application.Vendors.List;
using Application.Vendors.Lookup;
using Application.Vendors.Metrics;
using Application.Vendors.Reports;
using Application.Vendors.Search;
using Application.Vendors.Update;
using Domain.Vendors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class VendorController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public VendorController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorRequest body, ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var id = await sender.Send(new CreateVendorCommand(body.Name, body.Email, body.TaxId));
        return Ok(new { VendorId = id });
    }

    [HttpPut("{vendorId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateVendor(
        Guid vendorId,
        [FromBody] UpdateVendorRequest body,
        ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        await sender.Send(new UpdateVendorCommand(
            new VendorId(vendorId),
            body.Name,
            body.Email,
            body.TaxId));

        return Ok();
    }

    [HttpGet("List")]
    [ProducesResponseType(typeof(PagedVendorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedVendorResponse>> ListVendors(
        ISender sender,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new ListVendorsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("Search")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorSearchResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VendorSearchResult>>> SearchVendors(
        [FromQuery] string term,
        ISender sender)
    {
        var results = await sender.Send(new SearchVendorsQuery(term));
        return Ok(results);
    }

    [HttpGet("Export")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorExportRow>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VendorExportRow>>> ExportVendors(
        ISender sender,
        [FromQuery] bool skipValidation = false,
        [FromQuery] bool includeInternalFields = true)
    {
        var rows = await sender.Send(new ExportVendorsQuery(skipValidation, includeInternalFields));
        return Ok(rows);
    }

    [HttpGet("Metrics/UserOrderStats")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorMetricItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VendorMetricItem>>> GetUserOrderStats(ISender sender)
    {
        var stats = await sender.Send(new GetUserOrderStatsQuery());
        return Ok(stats);
    }

    [HttpGet("Lookup")]
    [ProducesResponseType(typeof(VendorLookupResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorLookupResult>> LookupVendor([FromQuery] string code, ISender sender)
    {
        var result = await sender.Send(new GetVendorByCodeQuery(code));
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{vendorId:guid}/Report")]
    [ProducesResponseType(typeof(VendorReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<VendorReportResponse>> GenerateReport(
        Guid vendorId,
        [FromQuery] DateOnly? date,
        ISender sender)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var report = await sender.Send(new GenerateVendorReportCommand(new VendorId(vendorId), reportDate));
        return Ok(report);
    }

    private bool IsAuthorized() => ApiKeyAuth.IsAuthorized(Request, _configuration);
}

public record CreateVendorRequest(string Name, string Email, string TaxId);

public record UpdateVendorRequest(string Name, string Email, string TaxId);
