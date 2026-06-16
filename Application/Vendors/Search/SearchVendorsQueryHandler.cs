using Domain.Vendors;
using MediatR;

namespace Application.Vendors.Search;

internal sealed class SearchVendorsQueryHandler : IRequestHandler<SearchVendorsQuery, IReadOnlyList<VendorSearchResult>>
{
    private readonly IVendorRepository _vendorRepository;

    public SearchVendorsQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public Task<IReadOnlyList<VendorSearchResult>> Handle(SearchVendorsQuery request, CancellationToken cancellationToken)
    {
        var vendors = Task.Run(
                () => _vendorRepository
                    .SearchUnsafeAsync(request.Term, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult(),
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        IReadOnlyList<VendorSearchResult> mapped = vendors
            .Select(v => new VendorSearchResult(v.Id.Value, v.Name, v.Email))
            .ToList();

        return Task.FromResult(mapped);
    }
}
