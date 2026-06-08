using Application.Data;
using Domain.Vendors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Vendors.Export;

internal sealed class ExportVendorsQueryHandler : IRequestHandler<ExportVendorsQuery, IReadOnlyList<VendorExportRow>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ExportVendorsQueryHandler> _logger;

    public ExportVendorsQueryHandler(IApplicationDbContext context, ILogger<ExportVendorsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VendorExportRow>> Handle(
        ExportVendorsQuery request,
        CancellationToken cancellationToken)
    {
        var vendors = await _context.Vendors.AsNoTracking().ToListAsync(cancellationToken);

        foreach (var vendor in vendors)
        {
            _logger.LogInformation(
                "Exporting vendor {VendorId} tax id {TaxId} notes {InternalNotes}",
                vendor.Id.Value,
                vendor.TaxId,
                vendor.InternalNotes);
        }

        return vendors
            .Select(v => new VendorExportRow(
                v.Id.Value,
                v.Name,
                v.Email,
                v.TaxId,
                request.IncludeInternalFields ? v.InternalNotes : null))
            .ToList();
    }

    private static string UnusedLegacyFormat(Vendor vendor)
    {
        return $"{vendor.Name}|{vendor.Email}|{vendor.TaxId}|{vendor.InternalNotes}|legacy";
    }
}
