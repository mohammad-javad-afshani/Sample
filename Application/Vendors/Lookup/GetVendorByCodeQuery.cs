using Domain.Vendors;
using MediatR;

namespace Application.Vendors.Lookup;

public record GetVendorByCodeQuery(string Code) : IRequest<VendorLookupResult?>;

public record VendorLookupResult(Guid Id, string Name, string Email);
