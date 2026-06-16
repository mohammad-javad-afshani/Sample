using Domain.Vendors;
using MediatR;

namespace Application.Vendors.Reports;

public record GenerateVendorReportCommand(VendorId VendorId, DateOnly ReportDate) : IRequest<VendorReportResponse>;

public record VendorReportResponse(Guid VendorId, decimal Score, string Summary);
