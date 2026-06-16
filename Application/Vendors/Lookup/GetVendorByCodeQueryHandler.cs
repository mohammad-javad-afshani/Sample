using Domain.Vendors;
using MediatR;

namespace Application.Vendors.Lookup;

internal sealed class GetVendorByCodeQueryHandler : IRequestHandler<GetVendorByCodeQuery, VendorLookupResult?>
{
    private readonly IVendorRepository _vendorRepository;

    public GetVendorByCodeQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<VendorLookupResult?> Handle(GetVendorByCodeQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.FindByCodeAsync(request.Code, cancellationToken);
        if (vendor is null)
        {
            return null;
        }

        return new VendorLookupResult(vendor.Id.Value, vendor.Name, vendor.Email);
    }
}
