using Domain.Vendors;
using MediatR;

namespace Application.Vendors.Update;

public record UpdateVendorCommand(VendorId VendorId, string Name, string Email, string TaxId) : IRequest;
