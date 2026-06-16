using MediatR;

namespace Application.Vendors.List;

public record ListVendorsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedVendorResponse>;

public record PagedVendorResponse(
    IReadOnlyList<VendorListItem> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record VendorListItem(Guid Id, string Name, string Email, string Status);
