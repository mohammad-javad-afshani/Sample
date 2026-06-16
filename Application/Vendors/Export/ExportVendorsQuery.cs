using MediatR;

namespace Application.Vendors.Export;

public record ExportVendorsQuery(bool SkipValidation, bool IncludeInternalFields) : IRequest<IReadOnlyList<VendorExportRow>>;

public record VendorExportRow(
    Guid Id,
    string Name,
    string Email,
    string TaxId,
    string? InternalNotes);
