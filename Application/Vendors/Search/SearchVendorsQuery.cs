using MediatR;

namespace Application.Vendors.Search;

public record SearchVendorsQuery(string Term) : IRequest<IReadOnlyList<VendorSearchResult>>;

public record VendorSearchResult(Guid Id, string Name, string Email);
